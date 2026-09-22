namespace Cms.Framework.Infrastructure;

/// <summary>
/// One language branch's publish state for a content item - enough to
/// classify it the same way a single-language summary is classified
/// (Published / Scheduled / Unpublished / Draft): the latest version in that
/// language, its start-publish, and whichever version (if any) is currently live.
/// </summary>
public sealed class LanguageStatusDto
{
    public required string Language { get; set; }
    public required int VersionNumber { get; set; }
    public DateTime? StartPublish { get; set; }

    /// <summary>The version number currently live in this language, or <c>null</c> if none is.</summary>
    public int? LivePublishedVersionNumber { get; set; }

    /// <summary>
    /// Whether ANY version of this language branch, ever, has had a publish window set - not just
    /// <see cref="VersionNumber"/>'s own. Distinguishes a branch that was published and later taken
    /// down (still <c>true</c> even once a fresh, never-published draft becomes the latest version)
    /// from one that has never been published at all.
    /// </summary>
    public required bool HasBeenPublished { get; set; }
}
