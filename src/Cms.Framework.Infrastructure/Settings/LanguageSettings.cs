namespace Cms.Framework.Infrastructure.Settings;

/// <summary>The available-languages CMS setting. Replaces the old, startup-only <c>ContentRepositoryOptions</c>.</summary>
public sealed class LanguageSettings
{
    /// <summary>Language pre-selected when creating new content.</summary>
    public string DefaultLanguage { get; set; } = "en";

    /// <summary>The languages the CMS offers for translating content.</summary>
    public IReadOnlyList<string> SupportedLanguages { get; set; } = new[] { "en" };

    public void Validate()
    {
        if (!SupportedLanguages.Contains(DefaultLanguage))
            throw new InvalidOperationException(
                $"DefaultLanguage '{DefaultLanguage}' must be one of SupportedLanguages ({string.Join(", ", SupportedLanguages)}).");
    }
}
