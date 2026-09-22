namespace Cms.Framework.Infrastructure.Settings;

/// <summary>
/// One persisted CMS setting, stored as an opaque JSON blob keyed by setting
/// name. Hand-written (not generated) and content-type-agnostic, following
/// the same standalone-table shape as <see cref="ContentRoot{TContentType}"/>.
/// </summary>
public sealed class CmsSettingEntry
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
