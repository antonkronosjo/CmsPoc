using Cms.Framework.Abstractions;

namespace Cms.Framework.Infrastructure.Editing;

/// <summary>
/// The single service the generic content-editing API talks to. Knows
/// nothing about any concrete content type at compile time - resolves them
/// by name (<see cref="IContentTypeMetadata{TContentType}.ContentTypeKey"/>) via
/// reflection over each type's <see cref="ContentPropertyAttribute"/>-decorated
/// properties. Everything below this service (root/version
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
    /// <param name="sortBy">
    /// A <see cref="ContentSummaryDto{TContentType}"/> property name (case-insensitive), e.g.
    /// <c>"name"</c> or <c>"lastModified"</c>. Unknown names or properties that aren't
    /// <see cref="IComparable"/> (such as <c>Languages</c>) are ignored, leaving results
    /// in their default order - callers only offer sorting for fields that are actually
    /// sortable, so this is a defensive fallback rather than a validation error.
    /// </param>
    /// <param name="contentTypeKeys">
    /// Filters to any of several content types at once (e.g. a news type plus its variants).
    /// Takes priority over <paramref name="contentTypeKey"/> when both are given.
    /// </param>
    /// <param name="publishedFrom">Only items whose language branch first went live (<see cref="ContentSummaryDto{TContentType}.FirstPublished"/>) on or after this instant.</param>
    /// <param name="publishedTo">Only items whose language branch first went live on or before this instant.</param>
    /// <param name="propertyName">
    /// A content type's own property name (case-insensitive) to range-filter by, e.g. an
    /// event's <c>StartDate</c>. Only <see cref="DateTime"/>-typed properties are comparable;
    /// items missing the property, or where it isn't a <see cref="DateTime"/>, are excluded.
    /// Ignored unless at least one of <paramref name="propertyValueFrom"/>/<paramref name="propertyValueTo"/> is given.
    /// </param>
    SearchContentResult<TContentType> Search(string? query, string? language, TContentType? contentTypeKey, int page, int pageSize, bool publishedOnly = false, string? sortBy = null, bool sortDescending = false, IReadOnlyCollection<TContentType>? contentTypeKeys = null, DateTime? publishedFrom = null, DateTime? publishedTo = null, string? propertyName = null, DateTime? propertyValueFrom = null, DateTime? propertyValueTo = null);

    /// <summary>Version history in <paramref name="language"/> (<c>null</c> = the item's master language).</summary>
    List<ContentSummaryDto<TContentType>> GetHistory(int id, string? language);

    /// <summary>
    /// Publishes a specific version of one language branch, replacing whatever was previously live in that language once its window is reached. Other languages are unaffected.
    /// A version that has already been live is not republished in place: a copy of it is added as a new version and published instead.
    /// Returns the version number actually published.
    /// </summary>
    int Publish(int id, string language, int versionNumber, DateTime? startPublish, DateTime? stopPublish);

    /// <summary>Stops whichever version is currently live in <paramref name="language"/> for this content item. No-op if none is.</summary>
    void Unpublish(int id, string language);

    /// <summary>
    /// GDPR unlinking: clears every stored reference to this user id across all
    /// content types. Versions and content are kept - only the attribution goes.
    /// </summary>
    void RemoveUserReferences(string userId);
}
