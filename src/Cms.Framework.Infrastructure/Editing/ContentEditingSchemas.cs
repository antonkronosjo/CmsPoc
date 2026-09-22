namespace Cms.Framework.Infrastructure.Editing;

/// <summary>A registered content type as listed to the UI: its key and the optional hex color from <c>[ContentType(Color = ...)]</c>.</summary>
public sealed class ContentTypeInfoDto
{
    public required string Key { get; set; }
    public string? Color { get; set; }
}

public sealed class CreateContentMetadata<TContentType>
    where TContentType : struct, Enum
{
    public required TContentType ContentTypeKey { get; set; }
    public required string Language { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class CreateContentSchema<TContentType>
    where TContentType : struct, Enum
{
    public required CreateContentMetadata<TContentType> Metadata { get; set; }
    public required Dictionary<string, ContentPropertyValueDto> Properties { get; set; }
}

public sealed class UpdateContentMetadata<TContentType>
    where TContentType : struct, Enum
{
    public required int Id { get; set; }
    public required TContentType ContentTypeKey { get; set; }
    public required string Language { get; set; }

    /// <summary>The language the item was created in - the only one where shared properties and the name can be edited.</summary>
    public required string MasterLanguage { get; set; }
    public required string Name { get; set; }
    public required int VersionNumber { get; set; }
    public required DateTime Created { get; set; }
    public DateTime? StartPublish { get; set; }
    public DateTime? StopPublish { get; set; }

    /// <summary>The version number currently live for this content item, or <c>null</c> if none is - may differ from <see cref="VersionNumber"/>.</summary>
    public int? LivePublishedVersionNumber { get; set; }

    /// <summary>Languages that have a branch (at least one version) for the item.</summary>
    public List<string> Languages { get; set; } = new();
}

/// <summary>
/// Returned by the update-schema GET and required as-is for the update PUT
/// body, and returned again by that same PUT - one type for the whole
/// round trip. The frontend holds exactly one <see cref="UpdateContentSchema{TContentType}"/>
/// in state, mutates <see cref="Properties"/> as the user types, and PUTs
/// the same object back unchanged in shape.
/// </summary>
public sealed class UpdateContentSchema<TContentType>
    where TContentType : struct, Enum
{
    public required UpdateContentMetadata<TContentType> Metadata { get; set; }
    public required Dictionary<string, ContentPropertyValueDto> Properties { get; set; }
}

/// <summary>
/// Lightweight row shape for search results, picker lookups, and version
/// history - callers that need to display content without a full editable
/// schema.
/// </summary>
public sealed class ContentSummaryDto<TContentType>
    where TContentType : struct, Enum
{
    public required int Id { get; set; }
    public required TContentType ContentTypeKey { get; set; }
    public required string Name { get; set; }
    public required string Language { get; set; }
    public required string MasterLanguage { get; set; }
    public required int VersionNumber { get; set; }
    public required DateTime Created { get; set; }
    public DateTime? StartPublish { get; set; }
    public DateTime? StopPublish { get; set; }

    /// <summary>The version number currently live for this content item, or <c>null</c> if none is - may differ from <see cref="VersionNumber"/>.</summary>
    public int? LivePublishedVersionNumber { get; set; }

    /// <summary>Languages that have a branch (at least one version) for the item. Filled by search and summary lookups, empty for history rows.</summary>
    public List<string> Languages { get; set; } = new();

    /// <summary>Who created this version, or <c>null</c> if unknown or user tracking is off.</summary>
    public UserRefDto? CreatedBy { get; set; }

    /// <summary>Who last changed this version's publish window, or <c>null</c> if unknown or user tracking is off.</summary>
    public UserRefDto? PublishedBy { get; set; }
    public Dictionary<string, object?> Properties { get; set; } = new();
}

/// <summary>
/// A reference to the user behind an action. <see cref="Id"/> is what the CMS
/// stored; <see cref="DisplayName"/> is looked up from the user adapter at
/// read time. <see cref="Removed"/> is <c>true</c> when the adapter no longer
/// knows this id (for example a user erased for GDPR) - show "Unknown user".
/// </summary>
public sealed class UserRefDto
{
    public required string Id { get; set; }
    public string? DisplayName { get; set; }
    public bool Removed { get; set; }
}

/// <summary>Request body for publishing a specific version of one language branch.</summary>
public sealed class PublishContentRequest
{
    /// <summary>The language branch whose version is published; other languages are unaffected.</summary>
    public required string Language { get; set; }

    public required int VersionNumber { get; set; }

    /// <summary>When the version should start being live. <c>null</c> means immediately.</summary>
    public DateTime? StartPublish { get; set; }

    /// <summary>When the version should stop being live. <c>null</c> means no scheduled end.</summary>
    public DateTime? StopPublish { get; set; }
}

/// <summary>
/// Paged search results, plus the total count of matches across all pages
/// (post-filtering) so callers can render pagination controls.
/// </summary>
public sealed class SearchContentResult<TContentType>
    where TContentType : struct, Enum
{
    public required List<ContentSummaryDto<TContentType>> Items { get; set; }
    public required int TotalCount { get; set; }
}
