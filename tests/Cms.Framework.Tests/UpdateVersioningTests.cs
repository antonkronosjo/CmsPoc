using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>Requirement #3/#13-#15: updating creates a new immutable version; the previous version remains available.</summary>
public sealed class UpdateVersioningTests : IDisposable
{
    private readonly ContentTestFixture _fixture = new();

    [Fact]
    public void Update_creates_a_new_version_and_leaves_the_previous_one_untouched()
    {
        var created = _fixture.Repository.Create(new NewsContent
        {
            Name = "HEJ",
            Heading = "Hello",
            Body = "World",
            Color = "red",
        }, "en");

        created.Color = "blue";
        var updated = _fixture.Repository.Update(created);

        Assert.Equal(2, updated.VersionNumber);
        Assert.Equal("blue", updated.Color);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal("HEJ", updated.Name); // Name lives on the (unversioned) root and must still be populated after Update

        var history = _fixture.Repository.QueryHistory<NewsContent>(created.Id, "en");
        Assert.Equal(2, history.Count);

        var version1 = history[0];
        Assert.Equal(1, version1.VersionNumber);
        Assert.Equal("red", version1.Color); // untouched

        var version2 = history[1];
        Assert.Equal(2, version2.VersionNumber);
        Assert.Equal("blue", version2.Color);

        // Unchanged culture-specific fields are copied forward into the new version, not left behind.
        Assert.Equal("Hello", version2.Heading);
        Assert.Equal("World", version2.Body);
    }

    [Fact]
    public void Current_version_query_always_reflects_the_latest_update()
    {
        var created = _fixture.Repository.Create(new NewsContent { Name = "HEJ", Heading = "H", Body = "B", Color = "red" }, "en");

        created.Color = "blue";
        _fixture.Repository.Update(created);

        created.Color = "green";
        var third = _fixture.Repository.Update(created);

        Assert.Equal(3, third.VersionNumber);

        var current = _fixture.Repository.Query<NewsContent>("en").Where(x => x.Id == created.Id).First();
        Assert.Equal(3, current.VersionNumber);
        Assert.Equal("green", current.Color);
    }

    public void Dispose() => _fixture.Dispose();
}
