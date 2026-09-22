namespace Cms.Framework.Abstractions;

/// <summary>
/// Base class for every developer-defined content type. A developer only ever
/// derives from this class and adds flat properties (optionally marked
/// <see cref="CultureSpecificAttribute"/>) - everything else (persistence
/// entities, EF configuration, per-language versioning, translations) is generated.
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
    /// <c>Update</c> to know which language branch a new version belongs to.
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// The language this item was created in. Populated by the repository;
    /// the shared (non-<see cref="CultureSpecificAttribute"/>) properties are
    /// only editable through this language. <see cref="Name"/> is
    /// culture-specific and editable in every language.
    /// </summary>
    public string MasterLanguage { get; set; } = string.Empty;

    /// <summary>1-based, sequential per content instance.</summary>
    public int VersionNumber { get; set; }

    public DateTime Created { get; set; }

    /// <summary>
    /// When this specific version starts being live, or <c>null</c> if it has
    /// never been published. All dates in this project are UTC.
    /// </summary>
    public DateTime? StartPublish { get; set; }

    /// <summary>
    /// When this specific version stops being live, or <c>null</c> for no
    /// scheduled end. All dates in this project are UTC.
    /// </summary>
    public DateTime? StopPublish { get; set; }

    /// <summary>
    /// Opaque id of the user who created this version, or <c>null</c> when
    /// user tracking is off or the reference has been removed. Names come
    /// from the user adapter, never from here.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Opaque id of the user who last changed this version's publish window,
    /// or <c>null</c> under the same conditions as <see cref="CreatedBy"/>.
    /// </summary>
    public string? PublishedBy { get; set; }
}
