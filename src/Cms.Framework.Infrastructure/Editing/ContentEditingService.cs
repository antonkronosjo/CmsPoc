using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Cms.Framework.Abstractions;
using Cms.Framework.Abstractions.Users;

namespace Cms.Framework.Infrastructure.Editing;

internal sealed class ContentEditingService<TContentType> : IContentEditingService
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

    public IReadOnlyList<string> GetContentTypes()
        => _contentTypes.Select(x => x.ContentTypeKey.ToString()).ToList();

    public CreateContentSchema GetCreationSchema(string contentTypeName, string language)
    {
        var metadata = ResolveContentType(ParseContentType(contentTypeName));
        return new CreateContentSchema
        {
            Metadata = new CreateContentMetadata { ContentTypeName = contentTypeName, Language = language },
            Properties = BuildPropertySchema(metadata.ClrType, instance: null),
        };
    }

    public UpdateContentSchema GetUpdateSchema(int id, string language, int? version = null)
    {
        if (version.HasValue)
        {
            var root = _db.ContentRoots.SingleOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Content '{id}' does not exist.");
            var metadata = ResolveContentType(root.ContentTypeKey);
            var historical = metadata.QueryHistory(_db, id, language).SingleOrDefault(x => x.VersionNumber == version.Value)
                ?? throw new KeyNotFoundException($"Version '{version.Value}' of content '{id}' does not exist in language '{language}'.");

            return ToUpdateSchema(historical, metadata, metadata.GetLivePublishedVersionNumber(_db, id));
        }

        var current = _contentRepository.Query<Content>(language).Where(x => x.Id == id).FirstOrDefault();
        if (current is not null)
        {
            var metadata = ResolveContentTypeByClrType(current.GetType());
            return ToUpdateSchema(current, metadata, metadata.GetLivePublishedVersionNumber(_db, id));
        }

        // The content exists but has no translation in this language yet -
        // a new language branch. Read root-level identity directly; Update
        // will copy every other language's data forward unchanged the
        // first time this branch is saved.
        var newBranchRoot = _db.ContentRoots.SingleOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Content '{id}' does not exist.");
        var rootMetadata = ResolveContentType(newBranchRoot.ContentTypeKey);

        return new UpdateContentSchema
        {
            Metadata = new UpdateContentMetadata
            {
                Id = newBranchRoot.Id,
                ContentTypeName = newBranchRoot.ContentTypeKey.ToString(),
                Language = language,
                Name = newBranchRoot.Name,
                VersionNumber = 0,
                Created = newBranchRoot.Created,
                LivePublishedVersionNumber = rootMetadata.GetLivePublishedVersionNumber(_db, id),
            },
            Properties = BuildPropertySchema(rootMetadata.ClrType, instance: null),
        };
    }

    private static UpdateContentSchema ToUpdateSchema(Content content, IContentTypeMetadata<TContentType> metadata, int? livePublishedVersionNumber)
        => new()
        {
            Metadata = new UpdateContentMetadata
            {
                Id = content.Id,
                ContentTypeName = metadata.ContentTypeKey.ToString(),
                Language = content.Language,
                Name = content.Name,
                VersionNumber = content.VersionNumber,
                Created = content.Created,
                StartPublish = content.StartPublish,
                StopPublish = content.StopPublish,
                LivePublishedVersionNumber = livePublishedVersionNumber,
            },
            Properties = BuildPropertySchema(content.GetType(), content),
        };

    public Content Create(CreateContentSchema request)
    {
        var userId = Authorize(CmsRole.Editor);
        var metadata = ResolveContentType(ParseContentType(request.Metadata.ContentTypeName));
        var instance = (Content)Activator.CreateInstance(metadata.ClrType)!;
        instance.Name = request.Metadata.Name;
        instance.Language = request.Metadata.Language;
        ApplyPropertyValues(instance, request.Properties);

        return metadata.Create(_db, instance, request.Metadata.Language, userId);
    }

    public Content Update(UpdateContentSchema request)
    {
        var userId = Authorize(CmsRole.Editor);
        var current = _contentRepository.Query<Content>(request.Metadata.Language).Where(x => x.Id == request.Metadata.Id).FirstOrDefault();

        Content instance;
        if (current is not null)
        {
            instance = current;
        }
        else
        {
            var root = _db.ContentRoots.SingleOrDefault(x => x.Id == request.Metadata.Id)
                ?? throw new KeyNotFoundException($"Content '{request.Metadata.Id}' does not exist.");
            var rootMetadata = ResolveContentType(root.ContentTypeKey);
            instance = (Content)Activator.CreateInstance(rootMetadata.ClrType)!;
            instance.Id = request.Metadata.Id;
        }

        instance.Name = request.Metadata.Name;
        instance.Language = request.Metadata.Language;
        ApplyPropertyValues(instance, request.Properties);

        var metadata = ResolveContentTypeByClrType(instance.GetType());
        return metadata.Update(_db, instance, userId);
    }

    public List<string> ValidateProperty(string contentTypeName, string propertyName, ContentPropertyValueDto value)
    {
        var metadata = ResolveContentType(ParseContentType(contentTypeName));
        var property = GetContentProperties(metadata.ClrType).SingleOrDefault(p => p.Name == propertyName)
            ?? throw new KeyNotFoundException($"'{propertyName}' is not an editable property of '{contentTypeName}'.");

        var resolvedValue = ResolveValue(property.PropertyType, value.Value);

        var errors = new List<string>();
        foreach (var attribute in property.GetCustomAttributes<ValidationAttribute>(inherit: true))
        {
            if (!attribute.IsValid(resolvedValue))
                errors.Add(attribute.FormatErrorMessage(propertyName));
        }
        return errors;
    }

    public ContentSummaryDto? GetSummary(int id, string language)
    {
        // Prefer the published version for display outside active editing;
        // fall back to the latest draft when nothing is published yet.
        var content = _contentRepository.Query<Content>(language, publishedOnly: true).Where(x => x.Id == id).FirstOrDefault()
            ?? _contentRepository.Query<Content>(language).Where(x => x.Id == id).FirstOrDefault();
        if (content is null) return null;

        var metadata = ResolveContentTypeByClrType(content.GetType());
        var summary = ToSummary(content, metadata.GetLivePublishedVersionNumber(_db, id));
        ResolveUsers([summary]);
        return summary;
    }

    public SearchContentResult Search(string? query, string language, string? contentTypeName, int page, int pageSize, bool publishedOnly = false)
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
            .Select(content => ToSummary(content, ResolveContentTypeByClrType(content.GetType()).GetLivePublishedVersionNumber(_db, content.Id)))
            .ToList();

        if (!string.IsNullOrWhiteSpace(contentTypeName))
            summaries = summaries.Where(x => x.ContentTypeName == contentTypeName).ToList();

        var items = summaries.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        ResolveUsers(items);

        return new SearchContentResult
        {
            Items = items,
            TotalCount = summaries.Count,
        };
    }

    public List<ContentSummaryDto> GetHistory(int id, string language)
    {
        var root = _db.ContentRoots.SingleOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Content '{id}' does not exist.");
        var metadata = ResolveContentType(root.ContentTypeKey);
        var livePublishedVersionNumber = metadata.GetLivePublishedVersionNumber(_db, id);

        var history = metadata.QueryHistory(_db, id, language).Select(content => ToSummary(content, livePublishedVersionNumber)).ToList();
        ResolveUsers(history);
        return history;
    }

    public void Publish(int id, int versionNumber, DateTime? startPublish, DateTime? stopPublish)
    {
        var userId = Authorize(CmsRole.Admin);
        var root = _db.ContentRoots.SingleOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Content '{id}' does not exist.");
        var metadata = ResolveContentType(root.ContentTypeKey);
        if (!metadata.VersionExists(_db, id, versionNumber))
            throw new KeyNotFoundException($"Version '{versionNumber}' of content '{id}' does not exist.");

        var start = startPublish ?? DateTime.UtcNow;

        // Whatever was live before this takes effect must stop exactly when
        // this version's window begins - otherwise it could resurface as
        // "live" again later (e.g. after this version is unpublished).
        metadata.StopActivePublish(_db, id, start, userId);
        metadata.SetPublishSchedule(_db, id, versionNumber, start, stopPublish, userId);

        foreach (var handler in _publishEventHandlers)
            handler.OnPublished(id, versionNumber, start, stopPublish);
    }

    public void Unpublish(int id)
    {
        var userId = Authorize(CmsRole.Admin);
        var root = _db.ContentRoots.SingleOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Content '{id}' does not exist.");
        var metadata = ResolveContentType(root.ContentTypeKey);
        var live = metadata.GetLivePublishedVersionNumber(_db, id);
        if (live is null) return;

        metadata.StopActivePublish(_db, id, DateTime.UtcNow, userId);

        foreach (var handler in _publishEventHandlers)
            handler.OnUnpublished(id, live.Value);
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
    private void ResolveUsers(IReadOnlyCollection<ContentSummaryDto> summaries)
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

    private ContentSummaryDto ToSummary(Content content, int? livePublishedVersionNumber)
    {
        var metadata = ResolveContentTypeByClrType(content.GetType());
        return new ContentSummaryDto
        {
            Id = content.Id,
            ContentTypeName = metadata.ContentTypeKey.ToString(),
            Name = content.Name,
            Language = content.Language,
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

    private void ApplyPropertyValues(Content instance, IDictionary<string, ContentPropertyValueDto> values)
    {
        var properties = GetContentProperties(instance.GetType()).ToDictionary(p => p.Name);
        foreach (var (name, dto) in values)
        {
            if (!properties.TryGetValue(name, out var property))
                throw new InvalidOperationException($"'{name}' is not an editable property of '{instance.GetType().Name}'.");

            property.SetValue(instance, ResolveValue(property.PropertyType, dto.Value));
        }
    }

    private static Dictionary<string, ContentPropertyValueDto> BuildPropertySchema(Type contentType, Content? instance)
        => GetContentProperties(contentType).ToDictionary(
            p => p.Name,
            p => new ContentPropertyValueDto
            {
                InputType = p.GetCustomAttribute<ContentPropertyAttribute>()!.InputType,
                Required = p.IsDefined(typeof(RequiredAttribute), inherit: true),
                Value = instance is not null ? p.GetValue(instance) : null,
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

    private static TContentType ParseContentType(string contentTypeName)
        => Enum.TryParse<TContentType>(contentTypeName, out var key) && key.ToString() == contentTypeName
            ? key
            : throw new KeyNotFoundException($"Content type '{contentTypeName}' is not registered.");

    private IContentTypeMetadata<TContentType> ResolveContentType(TContentType contentType)
        => _contentTypes.SingleOrDefault(x => EqualityComparer<TContentType>.Default.Equals(x.ContentTypeKey, contentType))
            ?? throw new KeyNotFoundException($"Content type '{contentType}' is not registered.");

    private IContentTypeMetadata<TContentType> ResolveContentTypeByClrType(Type clrType)
        => _contentTypes.Single(x => x.ClrType == clrType);
}
