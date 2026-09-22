using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Cms.Framework.Abstractions;
using Cms.Framework.Abstractions.Users;

namespace Cms.Framework.Infrastructure.Editing;

internal sealed class ContentEditingService<TContentType> : IContentEditingService<TContentType>
    where TContentType : struct, Enum
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly CmsDbContext<TContentType> _db;
    private readonly IContentRepository _contentRepository;
    private readonly List<IContentTypeMetadata<TContentType>> _contentTypes;
    private readonly List<IContentPublishEventHandler> _publishEventHandlers;
    private readonly ICmsUserAdapter? _userAdapter;

    public ContentEditingService(
        CmsDbContext<TContentType> db,
        IContentRepository contentRepository,
        IEnumerable<IContentTypeMetadata<TContentType>> contentTypes,
        IEnumerable<IContentPublishEventHandler> publishEventHandlers,
        ICmsUserAdapter? userAdapter = null)
    {
        _db = db;
        _contentRepository = contentRepository;
        _contentTypes = contentTypes.ToList();
        _publishEventHandlers = publishEventHandlers.ToList();
        _userAdapter = userAdapter;
    }

    /// <summary>
    /// Checks the current user holds <paramref name="requiredRole"/> and returns their
    /// id for attribution. With no user adapter registered, user tracking is off:
    /// nothing is enforced and <c>null</c> is returned.
    /// </summary>
    private string? Authorize(CmsRole requiredRole)
    {
        if (_userAdapter is null) return null;

        var user = _userAdapter.GetCurrentUser() ?? throw new CmsUnauthenticatedException();
        if (!user.IsInRole(requiredRole)) throw new CmsForbiddenException(requiredRole);
        return user.Id;
    }

    public IReadOnlyList<ContentTypeInfoDto> GetContentTypes()
        => _contentTypes.Select(x => new ContentTypeInfoDto { Key = x.ContentTypeKey.ToString(), Color = x.Color }).ToList();

    public CreateContentSchema<TContentType> GetCreationSchema(TContentType contentTypeKey, string language)
    {
        var metadata = ResolveContentType(contentTypeKey);
        return new CreateContentSchema<TContentType>
        {
            Metadata = new CreateContentMetadata<TContentType> { ContentTypeKey = metadata.ContentTypeKey, Language = language },
            Properties = BuildPropertySchema(metadata.ClrType, instance: null),
        };
    }

    public UpdateContentSchema<TContentType> GetUpdateSchema(int id, string language, int? version = null)
    {
        if (version.HasValue)
        {
            var root = _db.ContentRoots.SingleOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Content '{id}' does not exist.");
            var metadata = ResolveContentType(root.ContentTypeKey);
            var historical = metadata.QueryHistory(_db, id, language).SingleOrDefault(x => x.VersionNumber == version.Value)
                ?? throw new KeyNotFoundException($"Version '{version.Value}' of content '{id}' does not exist in language '{language}'.");

            return ToUpdateSchema(historical, metadata, metadata.GetLivePublishedVersionNumber(_db, id, language));
        }

        var current = _contentRepository.Query<Content>(language).Where(x => x.Id == id).FirstOrDefault();
        if (current is not null)
        {
            var metadata = ResolveContentTypeByClrType(current.GetType());
            return ToUpdateSchema(current, metadata, metadata.GetLivePublishedVersionNumber(_db, id, language));
        }
        // The content exists but this language has no branch yet. Start from the
        // master-language projection so the shared values are shown as they are;
        // the culture-specific values start blank. The first save creates the
        // branch at version 1, with its own history and publish state.
        // forward unchanged the first time this branch is saved.
        var master = _contentRepository.Query<Content>().Where(x => x.Id == id).FirstOrDefault()
            ?? throw new KeyNotFoundException($"Content '{id}' does not exist.");
        var masterMetadata = ResolveContentTypeByClrType(master.GetType());

        var schema = ToUpdateSchema(master, masterMetadata, livePublishedVersionNumber: null, blankCultureSpecific: true);
        schema.Metadata.Language = language;
        schema.Metadata.VersionNumber = 0;
        schema.Metadata.StartPublish = null;
        schema.Metadata.StopPublish = null;
        return schema;
    }

    private UpdateContentSchema<TContentType> ToUpdateSchema(Content content, IContentTypeMetadata<TContentType> metadata, int? livePublishedVersionNumber, bool blankCultureSpecific = false)
        => new()
        {
            Metadata = new UpdateContentMetadata<TContentType>
            {
                Id = content.Id,
                ContentTypeKey = metadata.ContentTypeKey,
                Language = content.Language,
                MasterLanguage = content.MasterLanguage,
                Name = content.Name,
                VersionNumber = content.VersionNumber,
                Created = content.Created,
                StartPublish = content.StartPublish,
                StopPublish = content.StopPublish,
                LivePublishedVersionNumber = livePublishedVersionNumber,
                Languages = LanguagesOf(metadata, content.Id),
            },
            Properties = BuildPropertySchema(content.GetType(), content, blankCultureSpecific),
        };

    private List<string> LanguagesOf(IContentTypeMetadata<TContentType> metadata, int id)
        => metadata.QueryLanguages(_db, [id]).TryGetValue(id, out var languages) ? languages.ToList() : new List<string>();

    public Content Create(CreateContentSchema<TContentType> request)
    {
        var userId = Authorize(CmsRole.Editor);
        var metadata = ResolveContentType(request.Metadata.ContentTypeKey);
        var instance = (Content)Activator.CreateInstance(metadata.ClrType)!;
        instance.Name = request.Metadata.Name;
        instance.Language = request.Metadata.Language;
        ApplyPropertyValues(instance, request.Properties);

        return metadata.Create(_db, instance, request.Metadata.Language, userId);
    }

    public Content Update(UpdateContentSchema<TContentType> request)
    {
        var userId = Authorize(CmsRole.Editor);
        var current = _contentRepository.Query<Content>(request.Metadata.Language).Where(x => x.Id == request.Metadata.Id).FirstOrDefault();

        // A language with no translation yet starts from the master-language
        // projection, so the shared values carry forward into the new version.
        var instance = current
            ?? _contentRepository.Query<Content>().Where(x => x.Id == request.Metadata.Id).FirstOrDefault()
            ?? throw new KeyNotFoundException($"Content '{request.Metadata.Id}' does not exist.");

        // Shared properties and the name are only editable in the master
        // language; edits to them from any other language are ignored.
        var isMaster = request.Metadata.Language == instance.MasterLanguage;
        if (isMaster) instance.Name = request.Metadata.Name;
        instance.Language = request.Metadata.Language;
        ApplyPropertyValues(instance, request.Properties, cultureSpecificOnly: !isMaster);

        var metadata = ResolveContentTypeByClrType(instance.GetType());
        return metadata.Update(_db, instance, userId);
    }

    public List<string> ValidateProperty(TContentType contentTypeKey, string propertyName, ContentPropertyValueDto value)
    {
        var metadata = ResolveContentType(contentTypeKey);
        var property = GetContentProperties(metadata.ClrType).SingleOrDefault(p => p.Name == propertyName)
            ?? throw new KeyNotFoundException($"'{propertyName}' is not an editable property of '{contentTypeKey}'.");

        var resolvedValue = ResolveValue(property.PropertyType, value.Value);

        var errors = new List<string>();
        foreach (var attribute in property.GetCustomAttributes<ValidationAttribute>(inherit: true))
        {
            if (!attribute.IsValid(resolvedValue))
                errors.Add(attribute.FormatErrorMessage(propertyName));
        }
        return errors;
    }

    public ContentSummaryDto<TContentType>? GetSummary(int id, string? language)
    {
        // Prefer the published version for display outside active editing;
        // fall back to the latest draft when nothing is published yet.
        Content? Find(string? lang)
            => _contentRepository.Query<Content>(lang, publishedOnly: true).Where(x => x.Id == id).FirstOrDefault()
                ?? _contentRepository.Query<Content>(lang).Where(x => x.Id == id).FirstOrDefault();

        // A requested language the item isn't translated into is a miss; only a null language
        // (each item in its master language) is guaranteed to find every item.
        var content = Find(language);
        if (content is null) return null;

        var metadata = ResolveContentTypeByClrType(content.GetType());
        var summary = ToSummary(content, metadata.GetLivePublishedVersionNumber(_db, id, content.Language));
        AttachLanguages([summary]);
        ResolveUsers([summary]);
        return summary;
    }

    public SearchContentResult<TContentType> Search(string? query, string? language, TContentType? contentTypeKey, int page, int pageSize, bool publishedOnly = false)
    {
        var results = _contentRepository.Query<Content>(language, publishedOnly).ToList();

        if (!publishedOnly)
        {
            // Admin/list view: show each item's published version when one
            // exists, falling back to its latest draft otherwise.
            var publishedById = _contentRepository.Query<Content>(language, publishedOnly: true).ToList().ToDictionary(x => x.Id);
            results = results.Select(x => publishedById.TryGetValue(x.Id, out var published) ? published : x).ToList();
        }

        if (!string.IsNullOrWhiteSpace(query))
            results = results.Where(x => x.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        var summaries = results
            .Select(content => ToSummary(content, ResolveContentTypeByClrType(content.GetType()).GetLivePublishedVersionNumber(_db, content.Id, content.Language)))
            .ToList();

        if (contentTypeKey is { } typeFilter)
            summaries = summaries.Where(x => EqualityComparer<TContentType>.Default.Equals(x.ContentTypeKey, typeFilter)).ToList();

        var items = summaries.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        AttachLanguages(items);
        ResolveUsers(items);

        return new SearchContentResult<TContentType>
        {
            Items = items,
            TotalCount = summaries.Count,
        };
    }

    /// <summary>Fills <see cref="ContentSummaryDto{TContentType}.Languages"/> with one query per content type present.</summary>
    private void AttachLanguages(IReadOnlyCollection<ContentSummaryDto<TContentType>> summaries)
    {
        foreach (var group in summaries.GroupBy(s => s.ContentTypeKey))
        {
            var metadata = ResolveContentType(group.Key);
            var languages = metadata.QueryLanguages(_db, group.Select(s => s.Id).ToList());
            foreach (var summary in group)
            {
                if (languages.TryGetValue(summary.Id, out var found)) summary.Languages = found.ToList();
            }
        }
    }

    public List<ContentSummaryDto<TContentType>> GetHistory(int id, string? language)
    {
        var root = _db.ContentRoots.SingleOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Content '{id}' does not exist.");
        var metadata = ResolveContentType(root.ContentTypeKey);
        var branch = language ?? root.MasterLanguage;
        var livePublishedVersionNumber = metadata.GetLivePublishedVersionNumber(_db, id, branch);

        var history = metadata.QueryHistory(_db, id, language).Select(content => ToSummary(content, livePublishedVersionNumber)).ToList();
        ResolveUsers(history);
        return history;
    }

    public void Publish(int id, string language, int versionNumber, DateTime? startPublish, DateTime? stopPublish)
    {
        var userId = Authorize(CmsRole.Admin);
        var root = _db.ContentRoots.SingleOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Content '{id}' does not exist.");
        var metadata = ResolveContentType(root.ContentTypeKey);
        if (!metadata.VersionExists(_db, id, language, versionNumber))
            throw new KeyNotFoundException($"Version '{versionNumber}' of content '{id}' does not exist in language '{language}'.");

        var start = startPublish ?? DateTime.UtcNow;

        // Whatever was live before this takes effect must stop exactly when
        // this version's window begins - otherwise it could resurface as
        // "live" again later (e.g. after this version is unpublished).
        metadata.StopActivePublish(_db, id, language, start, userId);
        metadata.SetPublishSchedule(_db, id, language, versionNumber, start, stopPublish, userId);

        foreach (var handler in _publishEventHandlers)
            handler.OnPublished(id, language, versionNumber, start, stopPublish);
    }

    public void Unpublish(int id, string language)
    {
        var userId = Authorize(CmsRole.Admin);
        var root = _db.ContentRoots.SingleOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Content '{id}' does not exist.");
        var metadata = ResolveContentType(root.ContentTypeKey);
        var live = metadata.GetLivePublishedVersionNumber(_db, id, language);
        if (live is null) return;

        metadata.StopActivePublish(_db, id, language, DateTime.UtcNow, userId);

        foreach (var handler in _publishEventHandlers)
            handler.OnUnpublished(id, language, live.Value);
    }

    public void RemoveUserReferences(string userId)
    {
        Authorize(CmsRole.Admin);
        foreach (var metadata in _contentTypes)
            metadata.RemoveUserReferences(_db, userId);
    }

    /// <summary>
    /// Fills in display names for the user references on <paramref name="summaries"/> with a
    /// single batch lookup. Ids the adapter no longer knows are marked
    /// <see cref="UserRefDto.Removed"/>. Without an adapter the raw ids are left as they are.
    /// </summary>
    private void ResolveUsers(IReadOnlyCollection<ContentSummaryDto<TContentType>> summaries)
    {
        if (_userAdapter is null) return;

        var references = summaries.SelectMany(s => new[] { s.CreatedBy, s.PublishedBy }).OfType<UserRefDto>().ToList();
        if (references.Count == 0) return;

        var profiles = _userAdapter.ResolveProfiles(references.Select(r => r.Id).Distinct());
        foreach (var reference in references)
        {
            if (profiles.TryGetValue(reference.Id, out var profile)) reference.DisplayName = profile.DisplayName;
            else reference.Removed = true;
        }
    }

    private static UserRefDto? ToUserRef(string? userId)
        => userId is null ? null : new UserRefDto { Id = userId };

    private ContentSummaryDto<TContentType> ToSummary(Content content, int? livePublishedVersionNumber)
    {
        var metadata = ResolveContentTypeByClrType(content.GetType());
        return new ContentSummaryDto<TContentType>
        {
            Id = content.Id,
            ContentTypeKey = metadata.ContentTypeKey,
            Name = content.Name,
            Language = content.Language,
            MasterLanguage = content.MasterLanguage,
            VersionNumber = content.VersionNumber,
            Created = content.Created,
            StartPublish = content.StartPublish,
            StopPublish = content.StopPublish,
            LivePublishedVersionNumber = livePublishedVersionNumber,
            CreatedBy = ToUserRef(content.CreatedBy),
            PublishedBy = ToUserRef(content.PublishedBy),
            Properties = GetContentProperties(content.GetType()).ToDictionary(p => p.Name, p => p.GetValue(content)),
        };
    }

    private void ApplyPropertyValues(Content instance, IDictionary<string, ContentPropertyValueDto> values, bool cultureSpecificOnly = false)
    {
        var properties = GetContentProperties(instance.GetType()).ToDictionary(p => p.Name);
        foreach (var (name, dto) in values)
        {
            if (!properties.TryGetValue(name, out var property))
                throw new InvalidOperationException($"'{name}' is not an editable property of '{instance.GetType().Name}'.");

            if (cultureSpecificOnly && !IsCultureSpecific(property)) continue;

            property.SetValue(instance, ResolveValue(property.PropertyType, dto.Value));
        }
    }

    private static bool IsCultureSpecific(PropertyInfo property)
        => property.IsDefined(typeof(CultureSpecificAttribute), inherit: false);

    private static Dictionary<string, ContentPropertyValueDto> BuildPropertySchema(Type contentType, Content? instance, bool blankCultureSpecific = false)
        => GetContentProperties(contentType).ToDictionary(
            p => p.Name,
            p => new ContentPropertyValueDto
            {
                InputType = p.GetCustomAttribute<ContentPropertyAttribute>()!.InputType,
                Required = p.IsDefined(typeof(RequiredAttribute), inherit: true),
                CultureSpecific = IsCultureSpecific(p),
                Value = instance is not null && !(blankCultureSpecific && IsCultureSpecific(p)) ? p.GetValue(instance) : null,
            });

    private static IEnumerable<PropertyInfo> GetContentProperties(Type type)
        => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite && p.IsDefined(typeof(ContentPropertyAttribute), inherit: true))
            // Reflection lists the most-derived type's properties first; show base-class
            // properties first instead (stable sort keeps declaration order within a class).
            .OrderBy(p => InheritanceDepth(p.DeclaringType!));

    private static int InheritanceDepth(Type type)
    {
        var depth = 0;
        for (var t = type.BaseType; t is not null; t = t.BaseType) depth++;
        return depth;
    }

    private static object? ResolveValue(Type targetType, object? rawValue)
    {
        if (rawValue is null)
            return null;

        if (rawValue is JsonElement jsonElement)
            return jsonElement.ValueKind == JsonValueKind.Null
                ? null
                : JsonSerializer.Deserialize(jsonElement.GetRawText(), targetType, JsonOptions);

        return rawValue;
    }

    private IContentTypeMetadata<TContentType> ResolveContentType(TContentType contentType)
        => _contentTypes.SingleOrDefault(x => EqualityComparer<TContentType>.Default.Equals(x.ContentTypeKey, contentType))
            ?? throw new KeyNotFoundException($"Content type '{contentType}' is not registered.");

    private IContentTypeMetadata<TContentType> ResolveContentTypeByClrType(Type clrType)
        => _contentTypes.Single(x => x.ClrType == clrType);
}
