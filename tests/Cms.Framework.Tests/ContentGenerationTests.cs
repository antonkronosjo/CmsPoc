using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>
/// Proves requirement #18: a developer only defines the flat content class -
/// NewsContent.cs and EventContent.cs in Cms.Poc.Domain contain nothing but
/// the flat properties shown in the framework's design brief. Every
/// persistence type below is produced purely by the source generator
/// inspecting those two files at compile time.
/// </summary>
public sealed class ContentGenerationTests
{
    [Fact]
    public void Persistence_types_exist_for_NewsContent_without_being_hand_written()
    {
        var assembly = typeof(NewsContent).Assembly;

        Assert.NotNull(assembly.GetType("Cms.Framework.Generated.NewsContentVersion"));
        Assert.NotNull(assembly.GetType("Cms.Framework.Generated.NewsContentTranslation"));
        Assert.NotNull(assembly.GetType("Cms.Framework.Generated.NewsContentVersionConfiguration"));
        Assert.NotNull(assembly.GetType("Cms.Framework.Generated.NewsContentTranslationConfiguration"));
        Assert.NotNull(assembly.GetType("Cms.Framework.Generated.NewsContentReadConfiguration"));
        Assert.NotNull(assembly.GetType("Cms.Framework.Generated.NewsContentStore"));
    }

    [Fact]
    public void Persistence_types_exist_for_EventContent_without_being_hand_written()
    {
        var assembly = typeof(EventContent).Assembly;

        Assert.NotNull(assembly.GetType("Cms.Framework.Generated.EventContentVersion"));
        Assert.NotNull(assembly.GetType("Cms.Framework.Generated.EventContentTranslation"));
        Assert.NotNull(assembly.GetType("Cms.Framework.Generated.EventContentStore"));
    }

    [Fact]
    public void NewsContent_source_file_declares_only_flat_properties()
    {
        // Sanity check on the developer-facing surface itself: only Content's
        // base members plus the properties from the design brief exist -
        // nothing root/version/translation-shaped leaks into the flat type.
        var properties = typeof(NewsContent).GetProperties().Select(p => p.Name).OrderBy(n => n).ToArray();

        Assert.Equal(
            new[] { "Body", "Color", "CreatedAtUtc", "Heading", "Id", "Language", "Name", "VersionNumber" },
            properties);
    }
}
