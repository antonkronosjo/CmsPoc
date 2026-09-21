namespace Cms.Framework.Abstractions.Users;

/// <summary>Display information for a user id, resolved on demand by the adapter and never persisted by the CMS.</summary>
public sealed record CmsUserProfile(string Id, string? DisplayName);
