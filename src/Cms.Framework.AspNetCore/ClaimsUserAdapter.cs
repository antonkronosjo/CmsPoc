using System.Security.Claims;
using Cms.Framework.Abstractions.Users;
using Cms.Framework.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Framework.AspNetCore;

/// <summary>
/// A ready-made <see cref="ICmsUserAdapter"/> for hosts that already sign
/// users in through ASP.NET Core authentication: the acting user is
/// <see cref="HttpContext.User"/>, identified by its <see cref="ClaimTypes.NameIdentifier"/>
/// claim, with <c>Admin</c> / <c>Editor</c> role claims mapped to <see cref="CmsRole"/>.
/// It has no user directory to consult, so it can only name the current user
/// (from the <see cref="ClaimTypes.Name"/> claim). Hosts with their own user
/// store should implement <see cref="ICmsUserAdapter"/> instead.
/// </summary>
internal sealed class ClaimsUserAdapter : ICmsUserAdapter
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClaimsUserAdapter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CmsUserIdentity? GetCurrentUser()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true) return null;

        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(id)) return null;

        var roles = new HashSet<CmsRole>();
        foreach (var role in Enum.GetValues<CmsRole>())
        {
            if (principal.IsInRole(role.ToString())) roles.Add(role);
        }
        return new CmsUserIdentity(id, roles);
    }

    public IReadOnlyDictionary<string, CmsUserProfile> ResolveProfiles(IEnumerable<string> ids)
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        var currentId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentId is null || !ids.Contains(currentId))
            return new Dictionary<string, CmsUserProfile>();

        return new Dictionary<string, CmsUserProfile>
        {
            [currentId] = new CmsUserProfile(currentId, principal!.FindFirstValue(ClaimTypes.Name)),
        };
    }
}

public static class CmsBuilderClaimsExtensions
{
    /// <summary>
    /// Uses the signed-in ASP.NET Core user as the CMS user; see <see cref="ClaimsUserAdapter"/>.
    /// Turns on user tracking and role enforcement.
    /// </summary>
    public static CmsBuilder UseClaimsUserAdapter(this CmsBuilder builder)
        => builder
            .ConfigureServices(services => services.AddHttpContextAccessor())
            .UseUserAdapter<ClaimsUserAdapter>();
}
