namespace Cms.Framework.Infrastructure.Settings;

/// <summary>The HTTP-facing surface for reading and admin-editing CMS settings.</summary>
public interface ICmsSettingsService
{
    /// <summary>Public read - no role required, matching how languages are already exposed to every visitor.</summary>
    LanguageSettings GetLanguageSettings();

    /// <summary>Admin-only. Throws <see cref="Abstractions.Settings.CmsSettingsNotConfiguredException"/> if no settings store is registered.</summary>
    void UpdateLanguageSettings(LanguageSettings settings);
}
