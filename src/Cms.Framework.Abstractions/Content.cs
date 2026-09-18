namespace Cms.Framework.Abstractions;

/// <summary>
/// Base class for every developer-defined content type. A developer only ever
/// derives from this class and adds flat properties (optionally marked
/// <see cref="CultureSpecificAttribute"/>) - everything else (persistence
/// entities, EF configuration, versioning, translations) is generated.
/// </summary>
public abstract class Content
{
    /// <summary>
    /// Globally unique across every content type. Backed by a single shared
    /// identity column, so no two content instances - regardless of type -
    /// can ever share an Id.
    /// </summary>
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The language this in-memory projection reflects. Populated by the
    /// repository whenever it materializes a flat object, and used by
    /// <c>Update</c> to know which translation row is being replaced.
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>1-based, sequential per content instance.</summary>
    public int VersionNumber { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
