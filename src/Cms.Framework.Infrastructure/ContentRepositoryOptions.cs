namespace Cms.Framework.Infrastructure;

/// <summary>Language settings configured through <see cref="CmsBuilder"/>.</summary>
public sealed class ContentRepositoryOptions
{
    /// <summary>Language pre-selected when creating new content.</summary>
    public string DefaultLanguage { get; set; } = "en";

    /// <summary>The languages the CMS offers for translating content.</summary>
    public IReadOnlyList<string> SupportedLanguages { get; set; } = new[] { "en" };
}
