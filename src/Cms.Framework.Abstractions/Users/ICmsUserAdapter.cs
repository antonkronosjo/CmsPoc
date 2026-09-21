namespace Cms.Framework.Abstractions.Users;

/// <summary>
/// The seam between the CMS and the consumer's own user store. Implement it
/// to have the CMS record who created and published each version and to
/// enforce <see cref="CmsRole"/>s. The CMS stores nothing but the opaque
/// <see cref="CmsUserIdentity.Id"/>; names and roles always come from here,
/// so erasing a user in the consumer's system needs no change in the CMS.
/// User tracking is optional: with no adapter registered nothing is
/// recorded and nothing is enforced.
/// </summary>
public interface ICmsUserAdapter
{
    /// <summary>The user performing the current operation, or <c>null</c> if there isn't an authenticated one.</summary>
    CmsUserIdentity? GetCurrentUser();

    /// <summary>
    /// Batch lookup of display information for the given ids, used only when
    /// showing history. Ids the adapter no longer knows (for example an
    /// erased user) must simply be left out of the result.
    /// </summary>
    IReadOnlyDictionary<string, CmsUserProfile> ResolveProfiles(IEnumerable<string> ids);
}
