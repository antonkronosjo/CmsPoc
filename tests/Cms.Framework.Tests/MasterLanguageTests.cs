using Cms.Framework.Abstractions;
using Cms.Framework.Infrastructure.Editing;
using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>
/// Every item has a master language (the one it was created in). Listing
/// without a language projects each item in its master language, and the
/// shared properties and name are only editable through that language.
/// </summary>
public sealed class MasterLanguageTests : IDisposable
{
    private readonly ContentTestFixture _fixture = new();

    private int CreateNews(string language, string name, string heading, string body = "Body")
    {
        var schema = _fixture.Editing.GetCreationSchema(ContentTypeKey.NewsContent, language);
        schema.Metadata.Name = name;
        schema.Properties["Heading"].Value = heading;
        schema.Properties["Body"].Value = body;
        schema.Properties["RelatedContent"].Value = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent);
        return _fixture.Editing.Create(schema).Id;
    }

    private void SaveTranslation(int id, string language, string heading)
    {
        var schema = _fixture.Editing.GetUpdateSchema(id, language);
        schema.Properties["Heading"].Value = heading;
        schema.Properties["Body"].Value = "Body " + language;
        _fixture.Editing.Update(schema);
    }

    [Fact]
    public void Create_records_the_creation_language_as_master_language()
    {
        var id = CreateNews("sv", "Nyhet", "Hej");

        var schema = _fixture.Editing.GetUpdateSchema(id, "sv");
        Assert.Equal("sv", schema.Metadata.MasterLanguage);
    }

    [Fact]
    public void Query_without_language_projects_each_item_in_its_own_master_language()
    {
        var english = CreateNews("en", "English item", "Hello");
        var swedish = CreateNews("sv", "Swedish item", "Hej");
        SaveTranslation(english, "sv", "Hej (sv)");

        var byId = _fixture.Repository.Query<NewsContent>().ToList().ToDictionary(x => x.Id);

        Assert.Equal("en", byId[english].Language);
        Assert.Equal("Hello", byId[english].Heading);
        Assert.Equal("sv", byId[swedish].Language);
        Assert.Equal("Hej", byId[swedish].Heading);
    }

    [Fact]
    public void Search_without_language_lists_every_item_with_its_translated_languages()
    {
        var english = CreateNews("en", "English item", "Hello");
        var swedish = CreateNews("sv", "Swedish item", "Hej");
        SaveTranslation(english, "sv", "Hej (sv)");

        var result = _fixture.Editing.Search(null, null, null, 1, 20);

        var items = result.Items.ToDictionary(x => x.Id);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(["en", "sv"], items[english].Languages);
        Assert.Equal("en", items[english].MasterLanguage);
        Assert.Equal(["sv"], items[swedish].Languages);
    }

    [Fact]
    public void Update_schema_flags_which_properties_are_culture_specific()
    {
        var id = CreateNews("en", "Item", "Hello");

        var properties = _fixture.Editing.GetUpdateSchema(id, "en").Properties;

        Assert.True(properties["Heading"].CultureSpecific);
        Assert.False(properties["RelatedContent"].CultureSpecific);
    }

    [Fact]
    public void New_language_schema_prefills_shared_values_and_blanks_translated_ones()
    {
        var id = CreateNews("en", "Item", "Hello");

        var schema = _fixture.Editing.GetUpdateSchema(id, "sv");

        Assert.Equal(0, schema.Metadata.VersionNumber);
        Assert.Equal("sv", schema.Metadata.Language);
        Assert.Equal("en", schema.Metadata.MasterLanguage);
        Assert.Equal(new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent), schema.Properties["RelatedContent"].Value);
        Assert.Null(schema.Properties["Heading"].Value);
    }

    [Fact]
    public void Saving_a_non_master_language_cannot_change_shared_properties_or_name()
    {
        var id = CreateNews("en", "Original name", "Hello");

        var schema = _fixture.Editing.GetUpdateSchema(id, "sv");
        schema.Metadata.Name = "Hacked name";
        schema.Properties["Heading"].Value = "Hej";
        schema.Properties["Body"].Value = "Varlden";
        schema.Properties["RelatedContent"].Value = new ContentReference<ContentTypeKey>(99, ContentTypeKey.NewsContent);
        _fixture.Editing.Update(schema);

        var master = _fixture.Editing.GetUpdateSchema(id, "en");
        Assert.Equal("Original name", master.Metadata.Name);
        Assert.Equal(new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent), master.Properties["RelatedContent"].Value);

        var swedish = _fixture.Editing.GetUpdateSchema(id, "sv");
        Assert.Equal("Hej", swedish.Properties["Heading"].Value);
    }

    [Fact]
    public void Saving_the_master_language_changes_shared_properties_and_name()
    {
        var id = CreateNews("en", "Original name", "Hello");

        var schema = _fixture.Editing.GetUpdateSchema(id, "en");
        schema.Metadata.Name = "New name";
        schema.Properties["RelatedContent"].Value = new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent);
        _fixture.Editing.Update(schema);

        var reread = _fixture.Editing.GetUpdateSchema(id, "en");
        Assert.Equal("New name", reread.Metadata.Name);
        Assert.Equal(new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent), reread.Properties["RelatedContent"].Value);
    }

    [Fact]
    public void GetSummary_is_null_for_a_language_the_item_is_not_translated_into()
    {
        var id = CreateNews("en", "Item", "Hello");

        Assert.Null(_fixture.Editing.GetSummary(id, "sv"));
    }

    [Fact]
    public void GetSummary_without_language_shows_the_master_language()
    {
        var id = CreateNews("sv", "Item", "Hej");

        var summary = _fixture.Editing.GetSummary(id, null);

        Assert.NotNull(summary);
        Assert.Equal("sv", summary.Language);
    }

    public void Dispose() => _fixture.Dispose();
}
