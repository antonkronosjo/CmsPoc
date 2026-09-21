namespace Cms.Framework.Infrastructure.Editing;

public sealed class CreateContentMetadata
{
    public required string ContentTypeName { get; set; }
    public required string Language { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class CreateContentSchema
{
    public required CreateContentMetadata Metadata { get; set; }
    public required Dictionary<string, ContentPropertyValueDto> Properties { get; set; }
}

public sealed class UpdateContentMetadata
{
    public required int Id { get; set; }
    public required string ContentTypeName { get; set; }
    public required string Language { get; set; }
    public required string Name { get; set; }
    public required int VersionNumber { get; set; }
    public required DateTime Created { get; set; }
    public DateTime? StartPublish { get; set; }
    public DateTime? StopPublish { get; set; }

    /// <summary>The version number currently live for this content item, or <c>null</c> if none is - may differ from <see cref="VersionNumber"/>.</summary>
    public int? LivePublishedVersionNumber { get; set; }
}

/// <summary>
/// Returned by the update-schema GET and required as-is for the update PUT
/// body, and returned again by that same PUT - one type for the whole
/// round trip. The frontend holds exactly one <see cref="UpdateContentSchema"/>
/// in state, mutates <see cref="Properties"/> as the user types, and PUTs
/// the same object back unchanged in shape.
/// </summary>
public sealed class UpdateContentSchema
{
    public required UpdateContentMetadata Metadata { get; set; }
    public required Dictionary<string, ContentPropertyValueDto> Properties { get; set; }
}

/// <summary>
/// Lightweight row shape for search results, picker lookups, and version
/// history - callers that need to display content without a full editable
/// schema.
/// </summary>
public sealed class ContentSummaryDto
{
    public required int Id { get; set; }
    public required string ContentTypeName { get; set; }
    public required string Name { get; set; }
    public required string Language { get; set; }
    public required int VersionNumber { get; set; }
    public required DateTime Created { get; set; }
    public DateTime? StartPublish { get; set; }
    public DateTime? StopPublish { get; set; }

    /// <summary>The version number currently live for this content item, or <c>null</c> if none is - may differ from <see cref="VersionNumber"/>.</summary>
    public int? LivePublishedVersionNumber { get; set; }
    public Dictionary<string, object?> Properties { get; set; } = new();
}

/// <summary>Request body for publishing a specific version.</summary>
public sealed class PublishContentRequest
{
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
public sealed class SearchContentResult
{
    public required List<ContentSummaryDto> Items { get; set; }
    public required int TotalCount { get; set; }
}
