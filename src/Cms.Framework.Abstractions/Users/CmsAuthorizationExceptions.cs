namespace Cms.Framework.Abstractions.Users;

/// <summary>Thrown when an operation needs a signed-in user and the adapter reports none.</summary>
public sealed class CmsUnauthenticatedException : Exception
{
    public CmsUnauthenticatedException()
        : base("This operation requires an authenticated user.")
    {
    }
}

/// <summary>Thrown when the current user lacks the role an operation requires.</summary>
public sealed class CmsForbiddenException : Exception
{
    public CmsForbiddenException(CmsRole requiredRole)
        : base($"This operation requires the '{requiredRole}' role.")
    {
        RequiredRole = requiredRole;
    }

    public CmsRole RequiredRole { get; }
}
