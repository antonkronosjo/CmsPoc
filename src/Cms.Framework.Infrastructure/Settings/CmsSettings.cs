using System.Text.Json;
using Cms.Framework.Abstractions.Settings;

namespace Cms.Framework.Infrastructure.Settings;

/// <summary>
/// Typed access to one CMS setting, backed by an optional <see cref="ICmsSettingsStore"/>.
/// With no store registered - or nothing saved there yet - <see cref="Get"/> returns
/// <paramref name="defaultValue"/> passed at registration (the "hard-coded at
/// initialization" path); <see cref="Set"/> then has nowhere to persist to and throws.
/// </summary>
public sealed class CmsSettings<TSettings> where TSettings : class
{
    private static readonly string Key = typeof(TSettings).Name;

    private readonly ICmsSettingsStore? _store;
    private readonly TSettings _defaultValue;

    public CmsSettings(ICmsSettingsStore? store, TSettings defaultValue)
    {
        _store = store;
        _defaultValue = defaultValue;
    }

    public TSettings Get()
    {
        var json = _store?.Get(Key);
        return json is null ? _defaultValue : JsonSerializer.Deserialize<TSettings>(json)!;
    }

    public void Set(TSettings value)
    {
        if (_store is null) throw new CmsSettingsNotConfiguredException();
        _store.Set(Key, JsonSerializer.Serialize(value));
    }
}
