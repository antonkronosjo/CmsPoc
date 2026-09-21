using Cms.Framework.Abstractions;
using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>Publishing a specific version, scheduling, and unpublishing.</summary>
public sealed class PublishingTests : IDisposable
{
    private readonly ContentTestFixture _fixture = new();

    [Fact]
    public void Publish_makes_the_version_appear_in_the_published_only_query()
    {
        var created = _fixture.Repository.Create(new NewsContent { Name = "A", Heading = "H", Body = "B", RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent) }, "en");

        Assert.Empty(_fixture.Repository.Query<Content>("en", publishedOnly: true).Where(x => x.Id == created.Id).ToList());

        _fixture.Editing.Publish(created.Id, created.VersionNumber, startPublish: null, stopPublish: null);

        var published = _fixture.Repository.Query<Content>("en", publishedOnly: true).Where(x => x.Id == created.Id).First();
        Assert.Equal(created.VersionNumber, published.VersionNumber);
    }

    [Fact]
    public void Publishing_a_later_started_version_supersedes_the_earlier_one()
    {
        var created = _fixture.Repository.Create(new NewsContent { Name = "A", Heading = "H", Body = "B", RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent) }, "en");
        created.RelatedContent = new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent);
        var v2 = _fixture.Repository.Update(created);

        var now = DateTime.UtcNow;
        _fixture.Editing.Publish(created.Id, 1, startPublish: now.AddMinutes(-10), stopPublish: null);
        _fixture.Editing.Publish(created.Id, v2.VersionNumber, startPublish: now.AddMinutes(-5), stopPublish: null);

        var live = _fixture.Repository.Query<Content>("en", publishedOnly: true).Where(x => x.Id == created.Id).First();
        Assert.Equal(v2.VersionNumber, live.VersionNumber);
    }

    [Fact]
    public void A_future_dated_publish_does_not_appear_until_its_start_passes()
    {
        var created = _fixture.Repository.Create(new NewsContent { Name = "A", Heading = "H", Body = "B", RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent) }, "en");

        _fixture.Editing.Publish(created.Id, created.VersionNumber, startPublish: DateTime.UtcNow.AddHours(1), stopPublish: null);

        Assert.Empty(_fixture.Repository.Query<Content>("en", publishedOnly: true).Where(x => x.Id == created.Id).ToList());
    }

    [Fact]
    public void StopPublish_excludes_a_version_once_its_window_has_ended()
    {
        var created = _fixture.Repository.Create(new NewsContent { Name = "A", Heading = "H", Body = "B", RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent) }, "en");
        var now = DateTime.UtcNow;

        _fixture.Editing.Publish(created.Id, created.VersionNumber, startPublish: now.AddMinutes(-10), stopPublish: now.AddMinutes(-5));

        Assert.Empty(_fixture.Repository.Query<Content>("en", publishedOnly: true).Where(x => x.Id == created.Id).ToList());
    }

    [Fact]
    public void Publishing_a_new_version_caps_the_previously_live_version_so_it_cannot_resurface_after_unpublish()
    {
        var created = _fixture.Repository.Create(new NewsContent { Name = "A", Heading = "H", Body = "B", RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent) }, "en");
        created.RelatedContent = new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent);
        var v2 = _fixture.Repository.Update(created);

        _fixture.Editing.Publish(created.Id, 1, startPublish: null, stopPublish: null);
        _fixture.Editing.Publish(created.Id, v2.VersionNumber, startPublish: null, stopPublish: null);
        _fixture.Editing.Unpublish(created.Id);

        Assert.Empty(_fixture.Repository.Query<Content>("en", publishedOnly: true).Where(x => x.Id == created.Id).ToList());
    }

    [Fact]
    public void Unpublish_stops_whichever_version_is_currently_live()
    {
        var created = _fixture.Repository.Create(new NewsContent { Name = "A", Heading = "H", Body = "B", RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent) }, "en");
        _fixture.Editing.Publish(created.Id, created.VersionNumber, startPublish: null, stopPublish: null);

        _fixture.Editing.Unpublish(created.Id);

        var summary = _fixture.Editing.GetSummary(created.Id, "en");
        Assert.Null(summary!.LivePublishedVersionNumber);
    }

    [Fact]
    public void Unpublish_is_a_no_op_when_nothing_is_currently_live()
    {
        var created = _fixture.Repository.Create(new NewsContent { Name = "A", Heading = "H", Body = "B", RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent) }, "en");

        _fixture.Editing.Unpublish(created.Id);

        var summary = _fixture.Editing.GetSummary(created.Id, "en");
        Assert.Null(summary!.LivePublishedVersionNumber);
    }

    [Fact]
    public void Publishing_a_nonexistent_version_throws()
    {
        var created = _fixture.Repository.Create(new NewsContent { Name = "A", Heading = "H", Body = "B", RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent) }, "en");

        Assert.Throws<KeyNotFoundException>(() => _fixture.Editing.Publish(created.Id, versionNumber: 99, startPublish: null, stopPublish: null));
    }

    [Fact]
    public void Polymorphic_published_only_query_spans_content_types()
    {
        var news = _fixture.Repository.Create(new NewsContent { Name = "N", Heading = "H", Body = "B", RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent) }, "en");
        var @event = _fixture.Repository.Create(new EventContent { Name = "E", Title = "T", Description = "D", StartDate = DateTime.UtcNow }, "en");
        var unpublishedNews = _fixture.Repository.Create(new NewsContent { Name = "N2", Heading = "H2", Body = "B2", RelatedContent = new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent) }, "en");

        _fixture.Editing.Publish(news.Id, news.VersionNumber, startPublish: null, stopPublish: null);
        _fixture.Editing.Publish(@event.Id, @event.VersionNumber, startPublish: null, stopPublish: null);

        var results = _fixture.Repository.Query<Content>("en", publishedOnly: true).ToList();

        Assert.Equal(2, results.Count);
        Assert.Contains(results, c => c.Id == news.Id);
        Assert.Contains(results, c => c.Id == @event.Id);
        Assert.DoesNotContain(results, c => c.Id == unpublishedNews.Id);
    }

    [Fact]
    public void Search_and_GetSummary_show_the_published_version_when_one_exists_and_fall_back_to_latest_otherwise()
    {
        var created = _fixture.Repository.Create(new NewsContent { Name = "A", Heading = "H", Body = "B", RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent) }, "en");
        created.RelatedContent = new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent);
        _fixture.Repository.Update(created);

        // No published version yet: both fall back to the latest draft.
        var draftSummary = _fixture.Editing.GetSummary(created.Id, "en");
        Assert.Equal(new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent), draftSummary!.Properties["RelatedContent"]);

        var draftSearch = _fixture.Editing.Search(query: null, "en", contentTypeName: null, page: 1, pageSize: 20);
        Assert.Equal(new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent), draftSearch.Items.Single(i => i.Id == created.Id).Properties["RelatedContent"]);

        _fixture.Editing.Publish(created.Id, 1, startPublish: null, stopPublish: null);

        var publishedSummary = _fixture.Editing.GetSummary(created.Id, "en");
        Assert.Equal(new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent), publishedSummary!.Properties["RelatedContent"]);

        var publishedSearch = _fixture.Editing.Search(query: null, "en", contentTypeName: null, page: 1, pageSize: 20);
        Assert.Equal(new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent), publishedSearch.Items.Single(i => i.Id == created.Id).Properties["RelatedContent"]);

        var publishedOnlySearch = _fixture.Editing.Search(query: null, "en", contentTypeName: null, page: 1, pageSize: 20, publishedOnly: true);
        Assert.Equal(new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent), publishedOnlySearch.Items.Single(i => i.Id == created.Id).Properties["RelatedContent"]);
    }

    public void Dispose() => _fixture.Dispose();
}
