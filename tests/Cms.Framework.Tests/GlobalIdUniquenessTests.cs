using Cms.Framework.Abstractions;
using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>Requirement #2/#6: content Ids are globally unique across every content type.</summary>
public sealed class GlobalIdUniquenessTests : IDisposable
{
    private readonly ContentTestFixture _fixture = new();

    [Fact]
    public void Ids_never_collide_across_different_content_types()
    {
        var news = _fixture.Repository.Create(new NewsContent { Name = "News", Heading = "H", Body = "B", RelatedContent = new ContentReference(1, "NewsContent") }, "en");
        var @event = _fixture.Repository.Create(new EventContent { Name = "Event", Title = "T", Description = "D", StartDate = DateTime.UtcNow }, "en");
        var news2 = _fixture.Repository.Create(new NewsContent { Name = "News2", Heading = "H2", Body = "B2", RelatedContent = new ContentReference(2, "NewsContent") }, "en");

        Assert.NotEqual(news.Id, @event.Id);
        Assert.NotEqual(@event.Id, news2.Id);
        Assert.NotEqual(news.Id, news2.Id);

        // Enforced by schema, not application bookkeeping: every content
        // instance - regardless of type - is a row in the single shared
        // ContentRoots table, so its Id comes from one identity column.
        Assert.Equal(3, _fixture.Db.ContentRoots.Count());
        Assert.Equal(new[] { news.Id, @event.Id, news2.Id }.OrderBy(x => x), _fixture.Db.ContentRoots.Select(r => r.Id).OrderBy(x => x));
    }

    public void Dispose() => _fixture.Dispose();
}
