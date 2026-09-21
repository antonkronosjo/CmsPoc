using Cms.Framework.Abstractions;
using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>Requirement #5/#11: Query&lt;Content&gt;() spans every content type and preserves the concrete runtime type.</summary>
public sealed class PolymorphicQueryTests : IDisposable
{
    private readonly ContentTestFixture _fixture = new();

    [Fact]
    public void Query_over_Content_finds_matching_instances_across_types_without_knowing_their_concrete_type()
    {
        var news = _fixture.Repository.Create(new NewsContent { Name = "HEJ", Heading = "Hello", Body = "World", RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent) }, "en");
        var @event = _fixture.Repository.Create(new EventContent { Name = "HEJ", Title = "T", Description = "D", StartDate = DateTime.UtcNow }, "en");
        _fixture.Repository.Create(new NewsContent { Name = "SOMETHING_ELSE", Heading = "X", Body = "Y", RelatedContent = new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent) }, "en");

        var results = _fixture.Repository.Query<Content>("en").Where(x => x.Name == "HEJ").ToList();

        Assert.Equal(2, results.Count);
        Assert.Contains(results, c => c is NewsContent nc && nc.Id == news.Id && nc.RelatedContent == new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent));
        Assert.Contains(results, c => c is EventContent ec && ec.Id == @event.Id);
        Assert.DoesNotContain(results, c => c.Name == "SOMETHING_ELSE");
    }

    [Fact]
    public void Query_over_Content_can_filter_by_Id()
    {
        var news = _fixture.Repository.Create(new NewsContent { Name = "A", Heading = "H", Body = "B", RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent) }, "en");
        _fixture.Repository.Create(new EventContent { Name = "B", Title = "T", Description = "D", StartDate = DateTime.UtcNow }, "en");

        var result = _fixture.Repository.Query<Content>("en").Where(x => x.Id == news.Id).First();

        Assert.IsType<NewsContent>(result);
    }

    [Fact]
    public void Query_over_Content_rejects_predicates_referencing_projection_only_members()
    {
        var query = _fixture.Repository.Query<Content>("en");

        Assert.Throws<NotSupportedException>(() => query.Where(x => x.Language == "en"));
    }

    public void Dispose() => _fixture.Dispose();
}
