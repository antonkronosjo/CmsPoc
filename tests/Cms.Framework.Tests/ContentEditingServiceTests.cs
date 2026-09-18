using Cms.Framework.Abstractions;
using Cms.Framework.Infrastructure.Editing;
using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>
/// Proves the generic, schema-driven editing layer (<see cref="IContentEditingService"/>)
/// works for a content type it only knows by name at runtime - the same
/// contract the HTTP API and frontend rely on.
/// </summary>
public sealed class ContentEditingServiceTests : IDisposable
{
    private readonly ContentTestFixture _fixture = new();

    [Fact]
    public void GetContentTypes_lists_every_registered_type()
    {
        var types = _fixture.Editing.GetContentTypes();

        Assert.Contains(nameof(NewsContent), types);
        Assert.Contains(nameof(EventContent), types);
    }

    [Fact]
    public void Create_by_type_name_persists_content_from_a_property_dictionary()
    {
        var schema = _fixture.Editing.GetCreationSchema(nameof(NewsContent), "en");
        schema.Metadata.Name = "HEJ";
        schema.Properties["Heading"].Value = "Hello";
        schema.Properties["Body"].Value = "World";
        schema.Properties["RelatedContentId"].Value = 1;

        var created = _fixture.Editing.Create(schema);

        var news = Assert.IsType<NewsContent>(created);
        Assert.True(news.Id > 0);
        Assert.Equal(1, news.VersionNumber);
        Assert.Equal("Hello", news.Heading);
        Assert.Equal("World", news.Body);
        Assert.Equal(1, news.RelatedContentId);
    }

    [Fact]
    public void Update_round_trips_through_the_same_schema_type()
    {
        var creationSchema = _fixture.Editing.GetCreationSchema(nameof(NewsContent), "en");
        creationSchema.Metadata.Name = "HEJ";
        creationSchema.Properties["Heading"].Value = "Hello";
        creationSchema.Properties["Body"].Value = "World";
        creationSchema.Properties["RelatedContentId"].Value = 1;
        var created = _fixture.Editing.Create(creationSchema);

        var updateSchema = _fixture.Editing.GetUpdateSchema(created.Id, "en");
        Assert.Equal(1, updateSchema.Properties["RelatedContentId"].Value);

        updateSchema.Properties["RelatedContentId"].Value = 2;
        var updated = _fixture.Editing.Update(updateSchema);

        var news = Assert.IsType<NewsContent>(updated);
        Assert.Equal(2, news.VersionNumber);
        Assert.Equal(2, news.RelatedContentId);

        // The same GET shape reflects the update immediately.
        var reread = _fixture.Editing.GetUpdateSchema(created.Id, "en");
        Assert.Equal(2, reread.Metadata.VersionNumber);
        Assert.Equal(2, reread.Properties["RelatedContentId"].Value);
    }

    [Fact]
    public void ValidateProperty_reports_required_violations_reusing_DataAnnotations()
    {
        var errorsForBlank = _fixture.Editing.ValidateProperty(
            nameof(NewsContent), "Heading", new ContentPropertyValueDto { InputType = InputType.Text, Required = true, Value = "" });
        Assert.NotEmpty(errorsForBlank);

        var errorsForFilled = _fixture.Editing.ValidateProperty(
            nameof(NewsContent), "Heading", new ContentPropertyValueDto { InputType = InputType.Text, Required = true, Value = "Hello" });
        Assert.Empty(errorsForFilled);
    }

    [Fact]
    public void GetUpdateSchema_falls_back_to_an_empty_schema_for_a_language_not_yet_translated()
    {
        var creationSchema = _fixture.Editing.GetCreationSchema(nameof(NewsContent), "en");
        creationSchema.Metadata.Name = "HEJ";
        creationSchema.Properties["Heading"].Value = "Hello";
        creationSchema.Properties["Body"].Value = "World";
        creationSchema.Properties["RelatedContentId"].Value = 1;
        var created = _fixture.Editing.Create(creationSchema);

        var swedishSchema = _fixture.Editing.GetUpdateSchema(created.Id, "sv");

        Assert.Equal(created.Id, swedishSchema.Metadata.Id);
        Assert.Equal(nameof(NewsContent), swedishSchema.Metadata.ContentTypeName);
        Assert.Equal("sv", swedishSchema.Metadata.Language);
        Assert.Null(swedishSchema.Properties["Heading"].Value);

        swedishSchema.Properties["Heading"].Value = "Hej";
        swedishSchema.Properties["Body"].Value = "Varlden";
        swedishSchema.Properties["RelatedContentId"].Value = 1;
        var updated = _fixture.Editing.Update(swedishSchema);

        Assert.Equal(2, updated.VersionNumber);

        // The English translation from before the new branch was added is untouched.
        var english = _fixture.Editing.GetUpdateSchema(created.Id, "en");
        Assert.Equal("Hello", english.Properties["Heading"].Value);
    }

    [Fact]
    public void Search_filters_by_content_type_and_paginates_results()
    {
        for (var i = 0; i < 3; i++)
        {
            var news = _fixture.Editing.GetCreationSchema(nameof(NewsContent), "en");
            news.Metadata.Name = $"News {i}";
            news.Properties["Heading"].Value = $"Heading {i}";
            news.Properties["Body"].Value = "Body";
            news.Properties["RelatedContentId"].Value = 1;
            _fixture.Editing.Create(news);
        }

        var eventSchema = _fixture.Editing.GetCreationSchema(nameof(EventContent), "en");
        eventSchema.Metadata.Name = "Event 0";
        eventSchema.Properties["Title"].Value = "Title";
        eventSchema.Properties["Description"].Value = "Description";
        _fixture.Editing.Create(eventSchema);

        var filtered = _fixture.Editing.Search(query: null, language: "en", contentTypeName: nameof(NewsContent), page: 1, pageSize: 20);
        Assert.Equal(3, filtered.TotalCount);
        Assert.All(filtered.Items, x => Assert.Equal(nameof(NewsContent), x.ContentTypeName));

        var firstPage = _fixture.Editing.Search(query: null, language: "en", contentTypeName: nameof(NewsContent), page: 1, pageSize: 2);
        var secondPage = _fixture.Editing.Search(query: null, language: "en", contentTypeName: nameof(NewsContent), page: 2, pageSize: 2);
        Assert.Equal(3, firstPage.TotalCount);
        Assert.Equal(2, firstPage.Items.Count);
        Assert.Equal(3, secondPage.TotalCount);
        Assert.Single(secondPage.Items);

        var unfiltered = _fixture.Editing.Search(query: null, language: "en", contentTypeName: null, page: 1, pageSize: 20);
        Assert.Equal(4, unfiltered.TotalCount);
    }

    [Fact]
    public void GetUpdateSchema_with_a_version_number_returns_that_version_not_the_current_one()
    {
        var creationSchema = _fixture.Editing.GetCreationSchema(nameof(NewsContent), "en");
        creationSchema.Metadata.Name = "HEJ";
        creationSchema.Properties["Heading"].Value = "Hello";
        creationSchema.Properties["Body"].Value = "World";
        creationSchema.Properties["RelatedContentId"].Value = 1;
        var created = _fixture.Editing.Create(creationSchema);

        var updateSchema = _fixture.Editing.GetUpdateSchema(created.Id, "en");
        updateSchema.Properties["RelatedContentId"].Value = 2;
        _fixture.Editing.Update(updateSchema);

        var version1 = _fixture.Editing.GetUpdateSchema(created.Id, "en", version: 1);
        Assert.Equal(1, version1.Metadata.VersionNumber);
        Assert.Equal(1, version1.Properties["RelatedContentId"].Value);

        var version2 = _fixture.Editing.GetUpdateSchema(created.Id, "en", version: 2);
        Assert.Equal(2, version2.Metadata.VersionNumber);
        Assert.Equal(2, version2.Properties["RelatedContentId"].Value);

        Assert.Throws<KeyNotFoundException>(() => _fixture.Editing.GetUpdateSchema(created.Id, "en", version: 99));
    }

    public void Dispose() => _fixture.Dispose();
}
