namespace Cms.Framework.Abstractions.Users;

/// <summary>
/// The roles the CMS understands. Deliberately a small fixed set: the
/// consumer's user adapter maps whatever roles its own user store has onto
/// these. <see cref="Admin"/> implies <see cref="Editor"/>.
/// </summary>
public enum CmsRole
{
    /// <summary>May create and update content.</summary>
    Editor,

    /// <summary>May do everything an editor can, and also publish and unpublish.</summary>
    Admin,
}
