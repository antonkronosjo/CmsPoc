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

    UpdateContentSchema GetUpdateSchema(int id, string language);

    Content Create(CreateContentSchema request);

    Content Update(UpdateContentSchema request);

    List<string> ValidateProperty(string contentTypeName, string propertyName, ContentPropertyValueDto value);

    ContentSummaryDto? GetSummary(int id, string language);

    List<ContentSummaryDto> Search(string? query, string language);

    List<ContentSummaryDto> GetHistory(int id, string language);
}
