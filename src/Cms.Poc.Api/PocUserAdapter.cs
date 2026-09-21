using Cms.Framework.Abstractions.Users;

namespace Cms.Poc.Api;

/// <summary>
/// Stand-in for a real user store, so the demo shows user tracking working
/// without needing sign-in. Every request acts as the same admin. A real
/// host would implement <see cref="ICmsUserAdapter"/> over its own users, or
/// call <c>UseClaimsUserAdapter()</c> once it has authentication.
/// </summary>
internal sealed class PocUserAdapter : ICmsUserAdapter
{
    private static readonly CmsUserProfile Admin = new("poc-admin", "Demo Admin");

    public CmsUserIdentity? GetCurrentUser()
        => new(Admin.Id, new HashSet<CmsRole> { CmsRole.Admin });

    public IReadOnlyDictionary<string, CmsUserProfile> ResolveProfiles(IEnumerable<string> ids)
        => ids.Contains(Admin.Id)
            ? new Dictionary<string, CmsUserProfile> { [Admin.Id] = Admin }
            : new Dictionary<string, CmsUserProfile>();
}
