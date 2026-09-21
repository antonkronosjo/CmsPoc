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
    public void OfTypes_restricts_the_polymorphic_query_to_the_given_content_types()
    {
        var news = _fixture.Repository.Create(new NewsContent { Name = "A", Heading = "H", Body = "B" }, "en");
        var @event = _fixture.Repository.Create(new EventContent { Name = "B", Title = "T", Description = "D", StartDate = DateTime.UtcNow }, "en");

        var onlyEvents = _fixture.Repository.Query<Content>("en").OfTypes(ContentTypeKey.EventContent).ToList();
        var both = _fixture.Repository.Query<Content>("en").OfTypes(ContentTypeKey.NewsContent, ContentTypeKey.EventContent).ToList();
        var none = _fixture.Repository.Query<Content>("en").OfTypes<ContentTypeKey>().ToList();

        Assert.Equal(@event.Id, Assert.IsType<EventContent>(Assert.Single(onlyEvents)).Id);
        Assert.Equal(new[] { news.Id, @event.Id }, both.Select(x => x.Id));
        Assert.Empty(none);
    }

    [Fact]
    public void OfTypes_is_rejected_on_a_concrete_type_query()
    {
        Assert.Throws<NotSupportedException>(() => _fixture.Repository.Query<NewsContent>("en").OfTypes(ContentTypeKey.NewsContent));
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
