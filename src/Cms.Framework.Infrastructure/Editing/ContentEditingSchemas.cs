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
    public required DateTime CreatedAtUtc { get; set; }
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
    public required DateTime CreatedAtUtc { get; set; }
    public Dictionary<string, object?> Properties { get; set; } = new();
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
