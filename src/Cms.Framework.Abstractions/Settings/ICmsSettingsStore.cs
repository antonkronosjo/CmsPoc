namespace Cms.Framework.Abstractions.Settings;

/// <summary>
/// The seam between the CMS and wherever CMS-wide settings (for example the
/// available languages) are persisted. Values are opaque, serialized blobs
/// keyed by setting name; <c>Cms.Framework.Infrastructure.CmsSettings&lt;TSettings&gt;</c>
/// owns the (de)serialization and the hard-coded fallback. Persisted settings
/// are optional: with no store registered, every setting simply stays at
/// whatever default was configured at startup - nothing is read or written.
/// </summary>
public interface ICmsSettingsStore
{
    /// <summary>The stored value for <paramref name="key"/>, or <c>null</c> if nothing has been saved yet.</summary>
    string? Get(string key);

    /// <summary>Persists <paramref name="value"/> for <paramref name="key"/>, replacing whatever was there.</summary>
    void Set(string key, string value);
}
