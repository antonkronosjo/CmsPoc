namespace Cms.Framework.Abstractions.Settings;

/// <summary>
/// Thrown when saving a setting is attempted but no <see cref="ICmsSettingsStore"/>
/// is registered. Reading always succeeds (falling back to the hard-coded
/// default), but there is nowhere to persist a write.
/// </summary>
public sealed class CmsSettingsNotConfiguredException : Exception
{
    public CmsSettingsNotConfiguredException()
        : base("No settings store is configured. Call UseDatabaseSettingsStore() or UseSettingsStore<T>() inside AddCms to allow settings to be saved.")
    {
    }
}
