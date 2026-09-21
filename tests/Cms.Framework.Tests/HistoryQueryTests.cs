using Cms.Framework.Abstractions;
using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>Requirement #16/#17: querying the current version as well as every historical version.</summary>
public sealed class HistoryQueryTests : IDisposable
{
    private readonly ContentTestFixture _fixture = new();

    [Fact]
    public void QueryHistory_returns_every_version_newest_first()
    {
        var created = _fixture.Repository.Create(new NewsContent { Name = "HEJ", Heading = "Hello", Body = "World", RelatedContent = new ContentReference(1, "NewsContent") }, "en");

        created.RelatedContent = new ContentReference(2, "NewsContent");
        _fixture.Repository.Update(created);

        created.RelatedContent = new ContentReference(3, "NewsContent");
        _fixture.Repository.Update(created);

        var history = _fixture.Repository.QueryHistory<NewsContent>(created.Id, "en");

        Assert.Equal(3, history.Count);
        Assert.Equal(new[] { 3, 2, 1 }, history.Select(h => h.VersionNumber));
        Assert.Equal(new ContentReference?[] { new ContentReference(3, "NewsContent"), new ContentReference(2, "NewsContent"), new ContentReference(1, "NewsContent") }, history.Select(h => h.RelatedContent));
    }

    public void Dispose() => _fixture.Dispose();
}
