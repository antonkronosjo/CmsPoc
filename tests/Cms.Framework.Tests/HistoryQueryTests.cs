using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>Requirement #16/#17: querying the current version as well as every historical version.</summary>
public sealed class HistoryQueryTests : IDisposable
{
    private readonly ContentTestFixture _fixture = new();

    [Fact]
    public void QueryHistory_returns_every_version_oldest_first()
    {
        var created = _fixture.Repository.Create(new NewsContent { Name = "HEJ", Heading = "Hello", Body = "World", Color = "red" }, "en");

        created.Color = "blue";
        _fixture.Repository.Update(created);

        created.Color = "green";
        _fixture.Repository.Update(created);

        var history = _fixture.Repository.QueryHistory<NewsContent>(created.Id, "en");

        Assert.Equal(3, history.Count);
        Assert.Equal(new[] { 1, 2, 3 }, history.Select(h => h.VersionNumber));
        Assert.Equal(new[] { "red", "blue", "green" }, history.Select(h => h.Color));
    }

    public void Dispose() => _fixture.Dispose();
}
