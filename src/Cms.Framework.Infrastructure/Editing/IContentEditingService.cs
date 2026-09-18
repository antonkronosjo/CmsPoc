using Cms.Framework.Abstractions;

namespace Cms.Framework.Infrastructure.Editing;

/// <summary>
/// The single service the generic content-editing API talks to. Knows
/// nothing about any concrete content type at compile time - resolves them
/// by name (<see cref="IContentTypeMetadata.ContentTypeKey"/>) via
/// reflection over each type's <see cref="ContentPropertyAttribute"/>-decorated
/// properties. Everything below this service (root/version/translation
/// tables, versioning) is still handled by <see cref="IContentRepository"/>
/// and the generated <see cref="IContentTypeStore{T}"/> per type - this
/// service only adds the schema/reflection layer needed to create and
/// update content generically.
/// </summary>
public interface IContentEditingService
{
    IReadOnlyList<string> GetContentTypes();

    CreateContentSchema GetCreationSchema(string contentTypeName, string language);

    UpdateContentSchema GetUpdateSchema(int id, string language, int? version = null);

    Content Create(CreateContentSchema request);

    Content Update(UpdateContentSchema request);

    List<string> ValidateProperty(string contentTypeName, string propertyName, ContentPropertyValueDto value);

    ContentSummaryDto? GetSummary(int id, string language);

    SearchContentResult Search(string? query, string language, string? contentTypeName, int page, int pageSize, bool publishedOnly = false);

    List<ContentSummaryDto> GetHistory(int id, string language);

    /// <summary>Publishes a specific version, replacing whatever was previously live once its window is reached.</summary>
    void Publish(int id, int versionNumber, DateTime? startPublish, DateTime? stopPublish);

    /// <summary>Stops whichever version is currently live for this content item. No-op if none is.</summary>
    void Unpublish(int id);
}
