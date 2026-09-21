namespace Cms.Framework.Abstractions.Users;

/// <summary>
/// Who is acting right now, as far as the CMS needs to know: an opaque id
/// (the only thing ever written to the CMS database) and their roles.
/// </summary>
/// <param name="Id">
/// An opaque identifier owned by the consumer's user store. Prefer a
/// surrogate id (for example a GUID) over an email or username - it is still
/// stored on every version the user touches.
/// </param>
/// <param name="Roles">The roles this user holds in the CMS.</param>
public sealed record CmsUserIdentity(string Id, IReadOnlySet<CmsRole> Roles)
{
    /// <summary>Whether this user holds <paramref name="role"/>, counting <see cref="CmsRole.Admin"/> as also being an editor.</summary>
    public bool IsInRole(CmsRole role)
        => Roles.Contains(role) || (role == CmsRole.Editor && Roles.Contains(CmsRole.Admin));
}
