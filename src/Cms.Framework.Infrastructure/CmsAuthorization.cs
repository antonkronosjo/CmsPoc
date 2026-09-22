using Cms.Framework.Abstractions.Users;

namespace Cms.Framework.Infrastructure;

/// <summary>
/// Shared role check for the CMS's own services. With no <see cref="ICmsUserAdapter"/>
/// registered, user tracking is off: nothing is enforced and <c>null</c> is returned.
/// </summary>
internal static class CmsAuthorization
{
    /// <summary>Checks the current user holds <paramref name="requiredRole"/> and returns their id for attribution.</summary>
    public static string? Authorize(ICmsUserAdapter? userAdapter, CmsRole requiredRole)
    {
        if (userAdapter is null) return null;

        var user = userAdapter.GetCurrentUser() ?? throw new CmsUnauthenticatedException();
        if (!user.IsInRole(requiredRole)) throw new CmsForbiddenException(requiredRole);
        return user.Id;
    }
}
