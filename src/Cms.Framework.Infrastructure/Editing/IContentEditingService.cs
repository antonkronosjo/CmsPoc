using Cms.Framework.Abstractions;

namespace Cms.Framework.Infrastructure.Editing;

/// <summary>
/// The single service the generic content-editing API talks to. Knows
/// nothing about any concrete content type at compile time - resolves them
/// by name (<see cref="IContentTypeMetadata{TContentType}.ContentTypeKey"/>) via
/// reflection over each type's <see cref="ContentPropertyAttribute"/>-decorated
/// properties. Everything below this service (root/version/translation
/// tables, versioning) is still handled by <see cref="IContentRepository"/>
/// and the generated <see cref="IContentTypeStore{T, TContentType}"/> per type - this
/// service only adds the schema/reflection layer needed to create and
/// update content generically.
/// <para>
/// When an <see cref="Abstractions.Users.ICmsUserAdapter"/> is registered, writes are
/// attributed to the current user and role-checked: create and update need
/// <see cref="Abstractions.Users.CmsRole.Editor"/>; publish, unpublish and
/// <see cref="RemoveUserReferences"/> need <see cref="Abstractions.Users.CmsRole.Admin"/>.
/// A failed check throws <see cref="Abstractions.Users.CmsUnauthenticatedException"/> or
/// <see cref="Abstractions.Users.CmsForbiddenException"/>. Reads are not restricted.
/// </para>
/// </summary>
public interface IContentEditingService<TContentType>
    where TContentType : struct, Enum
{
    IReadOnlyList<ContentTypeInfoDto> GetContentTypes();

    CreateContentSchema<TContentType> GetCreationSchema(TContentType contentTypeKey, string language);

    UpdateContentSchema<TContentType> GetUpdateSchema(int id, string language, int? version = null);

    Content Create(CreateContentSchema<TContentType> request);

    Content Update(UpdateContentSchema<TContentType> request);

    List<string> ValidateProperty(TContentType contentTypeKey, string propertyName, ContentPropertyValueDto value);

    /// <summary>
    /// The item as displayed in <paramref name="language"/>, or <c>null</c> when it has no
    /// translation in that language. With <paramref name="language"/> <c>null</c> it is shown in its master language.
    /// </summary>
    ContentSummaryDto<TContentType>? GetSummary(int id, string? language);

    /// <summary>
    /// Lists content. With <paramref name="language"/> <c>null</c> every item is listed,
    /// each in its own master language; otherwise only items translated into that language.
    /// </summary>
    SearchContentResult<TContentType> Search(string? query, string? language, TContentType? contentTypeKey, int page, int pageSize, bool publishedOnly = false);

    /// <summary>Version history in <paramref name="language"/> (<c>null</c> = the item's master language).</summary>
    List<ContentSummaryDto<TContentType>> GetHistory(int id, string? language);

    /// <summary>Publishes a specific version, replacing whatever was previously live once its window is reached.</summary>
    void Publish(int id, int versionNumber, DateTime? startPublish, DateTime? stopPublish);

    /// <summary>Stops whichever version is currently live for this content item. No-op if none is.</summary>
    void Unpublish(int id);

    /// <summary>
    /// GDPR unlinking: clears every stored reference to this user id across all
    /// content types. Versions and content are kept - only the attribution goes.
    /// </summary>
    void RemoveUserReferences(string userId);
}
