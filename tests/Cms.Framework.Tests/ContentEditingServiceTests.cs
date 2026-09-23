using Cms.Framework.Abstractions;
using Cms.Framework.Infrastructure.Editing;
using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>
/// Proves the generic, schema-driven editing layer (<see cref="IContentEditingService{TContentType}"/>)
/// works for a content type it only knows by name at runtime - the same
/// contract the HTTP API and frontend rely on.
/// </summary>
public sealed class ContentEditingServiceTests : IDisposable
{
    private readonly ContentTestFixture _fixture = new();

    [Fact]
    public void GetContentTypes_lists_every_registered_type()
    {
        var types = _fixture.Editing.GetContentTypes().Select(t => t.Key).ToList();

        Assert.Contains(nameof(NewsContent), types);
        Assert.Contains(nameof(EventContent), types);
    }

    [Fact]
    public void Create_by_type_name_persists_content_from_a_property_dictionary()
    {
        var schema = _fixture.Editing.GetCreationSchema(ContentTypeKey.NewsContent, "en");
        schema.Metadata.Name = "HEJ";
        schema.Properties["Heading"].Value = "Hello";
        schema.Properties["Body"].Value = "World";
        schema.Properties["RelatedContent"].Value = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent);

        var created = _fixture.Editing.Create(schema);

        var news = Assert.IsType<NewsContent>(created);
        Assert.True(news.Id > 0);
        Assert.Equal(1, news.VersionNumber);
        Assert.Equal("Hello", news.Heading);
        Assert.Equal("World", news.Body);
        Assert.Equal(new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent), news.RelatedContent);
    }

    [Fact]
    public void Update_round_trips_through_the_same_schema_type()
    {
        var creationSchema = _fixture.Editing.GetCreationSchema(ContentTypeKey.NewsContent, "en");
        creationSchema.Metadata.Name = "HEJ";
        creationSchema.Properties["Heading"].Value = "Hello";
        creationSchema.Properties["Body"].Value = "World";
        creationSchema.Properties["RelatedContent"].Value = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent);
        var created = _fixture.Editing.Create(creationSchema);

        var updateSchema = _fixture.Editing.GetUpdateSchema(created.Id, "en");
        Assert.Equal(new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent), updateSchema.Properties["RelatedContent"].Value);

        updateSchema.Properties["RelatedContent"].Value = new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent);
        var updated = _fixture.Editing.Update(updateSchema);

        var news = Assert.IsType<NewsContent>(updated);
        Assert.Equal(2, news.VersionNumber);
        Assert.Equal(new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent), news.RelatedContent);

        // The same GET shape reflects the update immediately.
        var reread = _fixture.Editing.GetUpdateSchema(created.Id, "en");
        Assert.Equal(2, reread.Metadata.VersionNumber);
        Assert.Equal(new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent), reread.Properties["RelatedContent"].Value);
    }

    [Fact]
    public void ValidateProperty_reports_required_violations_reusing_DataAnnotations()
    {
        var errorsForBlank = _fixture.Editing.ValidateProperty(
            ContentTypeKey.NewsContent, "Heading", new ContentPropertyValueDto { InputType = InputType.Text, Required = true, Value = "" });
        Assert.NotEmpty(errorsForBlank);

        var errorsForFilled = _fixture.Editing.ValidateProperty(
            ContentTypeKey.NewsContent, "Heading", new ContentPropertyValueDto { InputType = InputType.Text, Required = true, Value = "Hello" });
        Assert.Empty(errorsForFilled);
    }

    [Fact]
    public void GetUpdateSchema_falls_back_to_an_empty_schema_for_a_language_not_yet_translated()
    {
        var creationSchema = _fixture.Editing.GetCreationSchema(ContentTypeKey.NewsContent, "en");
        creationSchema.Metadata.Name = "HEJ";
        creationSchema.Properties["Heading"].Value = "Hello";
        creationSchema.Properties["Body"].Value = "World";
        creationSchema.Properties["RelatedContent"].Value = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent);
        var created = _fixture.Editing.Create(creationSchema);

        var swedishSchema = _fixture.Editing.GetUpdateSchema(created.Id, "sv");

        Assert.Equal(created.Id, swedishSchema.Metadata.Id);
        Assert.Equal(ContentTypeKey.NewsContent, swedishSchema.Metadata.ContentTypeKey);
        Assert.Equal("sv", swedishSchema.Metadata.Language);
        Assert.Null(swedishSchema.Properties["Heading"].Value);

        swedishSchema.Properties["Heading"].Value = "Hej";
        swedishSchema.Properties["Body"].Value = "Varlden";
        swedishSchema.Properties["RelatedContent"].Value = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent);
        var updated = _fixture.Editing.Update(swedishSchema);

        Assert.Equal(1, updated.VersionNumber); // a new language branch starts at its own version 1

        // The English branch is untouched.
        var english = _fixture.Editing.GetUpdateSchema(created.Id, "en");
        Assert.Equal("Hello", english.Properties["Heading"].Value);
    }

    [Fact]
    public void Search_filters_by_content_type_and_paginates_results()
    {
        for (var i = 0; i < 3; i++)
        {
            var news = _fixture.Editing.GetCreationSchema(ContentTypeKey.NewsContent, "en");
            news.Metadata.Name = $"News {i}";
            news.Properties["Heading"].Value = $"Heading {i}";
            news.Properties["Body"].Value = "Body";
            news.Properties["RelatedContent"].Value = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent);
            _fixture.Editing.Create(news);
        }

        var eventSchema = _fixture.Editing.GetCreationSchema(ContentTypeKey.EventContent, "en");
        eventSchema.Metadata.Name = "Event 0";
        eventSchema.Properties["Title"].Value = "Title";
        eventSchema.Properties["Description"].Value = "Description";
        _fixture.Editing.Create(eventSchema);

        var filtered = _fixture.Editing.Search(query: null, language: "en", contentTypeKey: ContentTypeKey.NewsContent, page: 1, pageSize: 20);
        Assert.Equal(3, filtered.TotalCount);
        Assert.All(filtered.Items, x => Assert.Equal(ContentTypeKey.NewsContent, x.ContentTypeKey));

        var firstPage = _fixture.Editing.Search(query: null, language: "en", contentTypeKey: ContentTypeKey.NewsContent, page: 1, pageSize: 2);
        var secondPage = _fixture.Editing.Search(query: null, language: "en", contentTypeKey: ContentTypeKey.NewsContent, page: 2, pageSize: 2);
        Assert.Equal(3, firstPage.TotalCount);
        Assert.Equal(2, firstPage.Items.Count);
        Assert.Equal(3, secondPage.TotalCount);
        Assert.Single(secondPage.Items);

        var unfiltered = _fixture.Editing.Search(query: null, language: "en", contentTypeKey: null, page: 1, pageSize: 20);
        Assert.Equal(4, unfiltered.TotalCount);
    }

    [Fact]
    public void Search_sorts_by_an_arbitrary_field_name_ascending_and_descending()
    {
        foreach (var name in new[] { "Charlie", "Alpha", "Bravo" })
        {
            var news = _fixture.Editing.GetCreationSchema(ContentTypeKey.NewsContent, "en");
            news.Metadata.Name = name;
            news.Properties["Heading"].Value = "Heading";
            news.Properties["Body"].Value = "Body";
            news.Properties["RelatedContent"].Value = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent);
            _fixture.Editing.Create(news);
        }

        var ascending = _fixture.Editing.Search(query: null, language: "en", contentTypeKey: null, page: 1, pageSize: 20, sortBy: "name");
        Assert.Equal(["Alpha", "Bravo", "Charlie"], ascending.Items.Select(x => x.Name));

        var descending = _fixture.Editing.Search(query: null, language: "en", contentTypeKey: null, page: 1, pageSize: 20, sortBy: "name", sortDescending: true);
        Assert.Equal(["Charlie", "Bravo", "Alpha"], descending.Items.Select(x => x.Name));
    }

    [Fact]
    public void Search_ignores_an_unknown_or_non_comparable_sortBy_and_falls_back_to_default_order()
    {
        var news = _fixture.Editing.GetCreationSchema(ContentTypeKey.NewsContent, "en");
        news.Metadata.Name = "Only item";
        news.Properties["Heading"].Value = "Heading";
        news.Properties["Body"].Value = "Body";
        news.Properties["RelatedContent"].Value = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent);
        _fixture.Editing.Create(news);

        var unknownField = _fixture.Editing.Search(query: null, language: "en", contentTypeKey: null, page: 1, pageSize: 20, sortBy: "notAField");
        Assert.Single(unknownField.Items);

        // Languages is a List<string> - not IComparable - so sorting by it should be a no-op, not a throw.
        var nonComparableField = _fixture.Editing.Search(query: null, language: "en", contentTypeKey: null, page: 1, pageSize: 20, sortBy: "languages");
        Assert.Single(nonComparableField.Items);
    }

    [Fact]
    public void GetUpdateSchema_with_a_version_number_returns_that_version_not_the_current_one()
    {
        var creationSchema = _fixture.Editing.GetCreationSchema(ContentTypeKey.NewsContent, "en");
        creationSchema.Metadata.Name = "HEJ";
        creationSchema.Properties["Heading"].Value = "Hello";
        creationSchema.Properties["Body"].Value = "World";
        creationSchema.Properties["RelatedContent"].Value = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent);
        var created = _fixture.Editing.Create(creationSchema);

        var updateSchema = _fixture.Editing.GetUpdateSchema(created.Id, "en");
        updateSchema.Properties["RelatedContent"].Value = new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent);
        _fixture.Editing.Update(updateSchema);

        var version1 = _fixture.Editing.GetUpdateSchema(created.Id, "en", version: 1);
        Assert.Equal(1, version1.Metadata.VersionNumber);
        Assert.Equal(new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent), version1.Properties["RelatedContent"].Value);

        var version2 = _fixture.Editing.GetUpdateSchema(created.Id, "en", version: 2);
        Assert.Equal(2, version2.Metadata.VersionNumber);
        Assert.Equal(new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent), version2.Properties["RelatedContent"].Value);

        Assert.Throws<KeyNotFoundException>(() => _fixture.Editing.GetUpdateSchema(created.Id, "en", version: 99));
    }

    public void Dispose() => _fixture.Dispose();
}
