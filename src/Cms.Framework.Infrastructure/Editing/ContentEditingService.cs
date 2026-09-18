using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Cms.Framework.Abstractions;

namespace Cms.Framework.Infrastructure.Editing;

internal sealed class ContentEditingService : IContentEditingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly CmsDbContext _db;
    private readonly IContentRepository _contentRepository;
    private readonly List<IContentTypeMetadata> _contentTypes;

    public ContentEditingService(CmsDbContext db, IContentRepository contentRepository, IEnumerable<IContentTypeMetadata> contentTypes)
    {
        _db = db;
        _contentRepository = contentRepository;
        _contentTypes = contentTypes.ToList();
    }

    public IReadOnlyList<string> GetContentTypes()
        => _contentTypes.Select(x => x.ContentTypeKey).ToList();

    public CreateContentSchema GetCreationSchema(string contentTypeName, string language)
    {
        var metadata = ResolveContentType(contentTypeName);
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

            return ToUpdateSchema(historical, metadata);
        }

        var current = _contentRepository.Query<Content>(language).Where(x => x.Id == id).FirstOrDefault();
        if (current is not null)
            return ToUpdateSchema(current, ResolveContentTypeByClrType(current.GetType()));

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
                ContentTypeName = newBranchRoot.ContentTypeKey,
                Language = language,
                Name = newBranchRoot.Name,
                VersionNumber = 0,
                CreatedAtUtc = newBranchRoot.CreatedAtUtc,
            },
            Properties = BuildPropertySchema(rootMetadata.ClrType, instance: null),
        };
    }

    private static UpdateContentSchema ToUpdateSchema(Content content, IContentTypeMetadata metadata)
        => new()
        {
            Metadata = new UpdateContentMetadata
            {
                Id = content.Id,
                ContentTypeName = metadata.ContentTypeKey,
                Language = content.Language,
                Name = content.Name,
                VersionNumber = content.VersionNumber,
                CreatedAtUtc = content.CreatedAtUtc,
            },
            Properties = BuildPropertySchema(content.GetType(), content),
        };

    public Content Create(CreateContentSchema request)
    {
        var metadata = ResolveContentType(request.Metadata.ContentTypeName);
        var instance = (Content)Activator.CreateInstance(metadata.ClrType)!;
        instance.Name = request.Metadata.Name;
        instance.Language = request.Metadata.Language;
        ApplyPropertyValues(instance, request.Properties);

        return metadata.Create(_db, instance, request.Metadata.Language);
    }

    public Content Update(UpdateContentSchema request)
    {
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
        return metadata.Update(_db, instance);
    }

    public List<string> ValidateProperty(string contentTypeName, string propertyName, ContentPropertyValueDto value)
    {
        var metadata = ResolveContentType(contentTypeName);
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
        var current = _contentRepository.Query<Content>(language).Where(x => x.Id == id).FirstOrDefault();
        return current is null ? null : ToSummary(current);
    }

    public SearchContentResult Search(string? query, string language, string? contentTypeName, int page, int pageSize)
    {
        var results = _contentRepository.Query<Content>(language).ToList();

        if (!string.IsNullOrWhiteSpace(query))
            results = results.Where(x => x.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        var summaries = results.Select(ToSummary).ToList();

        if (!string.IsNullOrWhiteSpace(contentTypeName))
            summaries = summaries.Where(x => x.ContentTypeName == contentTypeName).ToList();

        return new SearchContentResult
        {
            Items = summaries.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            TotalCount = summaries.Count,
        };
    }

    public List<ContentSummaryDto> GetHistory(int id, string language)
    {
        var root = _db.ContentRoots.SingleOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Content '{id}' does not exist.");
        var metadata = ResolveContentType(root.ContentTypeKey);

        return metadata.QueryHistory(_db, id, language).Select(ToSummary).ToList();
    }

    private ContentSummaryDto ToSummary(Content content)
    {
        var metadata = ResolveContentTypeByClrType(content.GetType());
        return new ContentSummaryDto
        {
            Id = content.Id,
            ContentTypeName = metadata.ContentTypeKey,
            Name = content.Name,
            Language = content.Language,
            VersionNumber = content.VersionNumber,
            CreatedAtUtc = content.CreatedAtUtc,
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
            .Where(p => p.CanRead && p.CanWrite && p.IsDefined(typeof(ContentPropertyAttribute), inherit: true));

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

    private IContentTypeMetadata ResolveContentType(string contentTypeName)
        => _contentTypes.SingleOrDefault(x => x.ContentTypeKey == contentTypeName)
            ?? throw new KeyNotFoundException($"Content type '{contentTypeName}' is not registered.");

    private IContentTypeMetadata ResolveContentTypeByClrType(Type clrType)
        => _contentTypes.Single(x => x.ClrType == clrType);
}
