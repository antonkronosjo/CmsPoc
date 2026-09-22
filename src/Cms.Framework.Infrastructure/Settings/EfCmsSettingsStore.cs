using Cms.Framework.Abstractions.Settings;

namespace Cms.Framework.Infrastructure.Settings;

/// <summary>
/// <see cref="ICmsSettingsStore"/> backed by <see cref="CmsDbContext{TContentType}"/>.
/// Generic only because that's what depending on the shared DbContext requires -
/// settings themselves have nothing to do with any content type.
/// </summary>
internal sealed class EfCmsSettingsStore<TContentType> : ICmsSettingsStore where TContentType : struct, Enum
{
    private readonly CmsDbContext<TContentType> _db;

    public EfCmsSettingsStore(CmsDbContext<TContentType> db)
    {
        _db = db;
    }

    public string? Get(string key)
        => _db.SettingEntries.SingleOrDefault(x => x.Key == key)?.Value;

    public void Set(string key, string value)
    {
        var entry = _db.SettingEntries.SingleOrDefault(x => x.Key == key);
        if (entry is null)
            _db.SettingEntries.Add(new CmsSettingEntry { Key = key, Value = value });
        else
            entry.Value = value;

        _db.SaveChanges();
    }
}
