using Cms.Framework.Abstractions.Users;

namespace Cms.Framework.Infrastructure.Settings;

internal sealed class CmsSettingsService : ICmsSettingsService
{
    private readonly CmsSettings<LanguageSettings> _languageSettings;
    private readonly ICmsUserAdapter? _userAdapter;

    public CmsSettingsService(CmsSettings<LanguageSettings> languageSettings, ICmsUserAdapter? userAdapter = null)
    {
        _languageSettings = languageSettings;
        _userAdapter = userAdapter;
    }

    public LanguageSettings GetLanguageSettings() => _languageSettings.Get();

    public void UpdateLanguageSettings(LanguageSettings settings)
    {
        CmsAuthorization.Authorize(_userAdapter, CmsRole.Admin);
        settings.Validate();
        _languageSettings.Set(settings);
    }
}
