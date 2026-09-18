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
        schema.Properties["Color"].Value = "red";

        var created = _fixture.Editing.Create(schema);

        var news = Assert.IsType<NewsContent>(created);
        Assert.True(news.Id > 0);
        Assert.Equal(1, news.VersionNumber);
        Assert.Equal("Hello", news.Heading);
        Assert.Equal("World", news.Body);
        Assert.Equal("red", news.Color);
    }

    [Fact]
    public void Update_round_trips_through_the_same_schema_type()
    {
        var creationSchema = _fixture.Editing.GetCreationSchema(nameof(NewsContent), "en");
        creationSchema.Metadata.Name = "HEJ";
        creationSchema.Properties["Heading"].Value = "Hello";
        creationSchema.Properties["Body"].Value = "World";
        creationSchema.Properties["Color"].Value = "red";
        var created = _fixture.Editing.Create(creationSchema);

        var updateSchema = _fixture.Editing.GetUpdateSchema(created.Id, "en");
        Assert.Equal("red", updateSchema.Properties["Color"].Value);

        updateSchema.Properties["Color"].Value = "blue";
        var updated = _fixture.Editing.Update(updateSchema);

        var news = Assert.IsType<NewsContent>(updated);
        Assert.Equal(2, news.VersionNumber);
        Assert.Equal("blue", news.Color);

        // The same GET shape reflects the update immediately.
        var reread = _fixture.Editing.GetUpdateSchema(created.Id, "en");
        Assert.Equal(2, reread.Metadata.VersionNumber);
        Assert.Equal("blue", reread.Properties["Color"].Value);
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
        creationSchema.Properties["Color"].Value = "red";
        var created = _fixture.Editing.Create(creationSchema);

        var swedishSchema = _fixture.Editing.GetUpdateSchema(created.Id, "sv");

        Assert.Equal(created.Id, swedishSchema.Metadata.Id);
        Assert.Equal(nameof(NewsContent), swedishSchema.Metadata.ContentTypeName);
        Assert.Equal("sv", swedishSchema.Metadata.Language);
        Assert.Null(swedishSchema.Properties["Heading"].Value);

        swedishSchema.Properties["Heading"].Value = "Hej";
        swedishSchema.Properties["Body"].Value = "Varlden";
        swedishSchema.Properties["Color"].Value = "red";
        var updated = _fixture.Editing.Update(swedishSchema);

        Assert.Equal(2, updated.VersionNumber);

        // The English translation from before the new branch was added is untouched.
        var english = _fixture.Editing.GetUpdateSchema(created.Id, "en");
        Assert.Equal("Hello", english.Properties["Heading"].Value);
    }

    public void Dispose() => _fixture.Dispose();
}
