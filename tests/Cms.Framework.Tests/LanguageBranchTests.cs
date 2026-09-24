using Cms.Framework.Abstractions;
using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>Every language is its own branch, with its own version history and publish state.</summary>
public sealed class LanguageBranchTests : IDisposable
{
    private readonly ContentTestFixture _fixture = new();

    private NewsContent CreateWithSwedishBranch()
    {
        var created = _fixture.Repository.Create(new NewsContent
        {
            Name = "A",
            Heading = "Hello",
            Body = "World",
            RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent),
        }, "en");

        _fixture.Repository.Update(new NewsContent
        {
            Id = created.Id,
            Name = created.Name,
            Language = "sv",
            RelatedContent = created.RelatedContent,
            Heading = "Hej",
            Body = "Varlden",
        });
        return created;
    }

    [Fact]
    public void Publishing_one_language_does_not_publish_the_others()
    {
        var created = CreateWithSwedishBranch();

        _fixture.Editing.Publish(created.Id, "sv", 1, startPublish: null, stopPublish: null);

        Assert.Single(_fixture.Repository.Query<NewsContent>("sv", publishedOnly: true).Where(x => x.Id == created.Id).ToList());
        Assert.Empty(_fixture.Repository.Query<NewsContent>("en", publishedOnly: true).Where(x => x.Id == created.Id).ToList());
        Assert.Null(_fixture.Editing.GetSummary(created.Id, "en")!.LivePublishedVersionNumber);
        Assert.Equal(1, _fixture.Editing.GetSummary(created.Id, "sv")!.LivePublishedVersionNumber);
    }

    [Fact]
    public void Each_language_has_its_own_first_published_date()
    {
        var created = CreateWithSwedishBranch();
        var now = DateTime.UtcNow;

        _fixture.Editing.Publish(created.Id, "en", 1, startPublish: now.AddDays(-3), stopPublish: null);
        _fixture.Editing.Publish(created.Id, "sv", 1, startPublish: now.AddDays(-1), stopPublish: null);

        Assert.Equal(now.AddDays(-3), _fixture.Editing.GetSummary(created.Id, "en")!.FirstPublished);
        Assert.Equal(now.AddDays(-1), _fixture.Editing.GetSummary(created.Id, "sv")!.FirstPublished);
    }

    [Fact]
    public void Unpublishing_one_language_leaves_the_others_live()
    {
        var created = CreateWithSwedishBranch();
        _fixture.Editing.Publish(created.Id, "en", 1, startPublish: null, stopPublish: null);
        _fixture.Editing.Publish(created.Id, "sv", 1, startPublish: null, stopPublish: null);

        _fixture.Editing.Unpublish(created.Id, "sv");

        Assert.Empty(_fixture.Repository.Query<NewsContent>("sv", publishedOnly: true).Where(x => x.Id == created.Id).ToList());
        Assert.Single(_fixture.Repository.Query<NewsContent>("en", publishedOnly: true).Where(x => x.Id == created.Id).ToList());
    }

    [Fact]
    public void Each_language_has_its_own_version_numbering_and_history()
    {
        var created = CreateWithSwedishBranch();

        var swedish = _fixture.Repository.Query<NewsContent>("sv").Where(x => x.Id == created.Id).First();
        swedish.Heading = "Hej igen";
        _fixture.Repository.Update(swedish);

        Assert.Equal(new[] { 2, 1 }, _fixture.Repository.QueryHistory<NewsContent>(created.Id, "sv").Select(x => x.VersionNumber));
        Assert.Equal(new[] { 1 }, _fixture.Repository.QueryHistory<NewsContent>(created.Id, "en").Select(x => x.VersionNumber));
    }

    [Fact]
    public void Publishing_a_version_number_that_only_exists_in_another_language_throws()
    {
        var created = CreateWithSwedishBranch();
        var swedish = _fixture.Repository.Query<NewsContent>("sv").Where(x => x.Id == created.Id).First();
        swedish.Heading = "Hej igen";
        _fixture.Repository.Update(swedish);

        Assert.Throws<KeyNotFoundException>(() => _fixture.Editing.Publish(created.Id, "en", 2, startPublish: null, stopPublish: null));
    }

    [Fact]
    public void Non_master_branches_never_store_shared_values_but_resolve_them_live_from_the_master_branch()
    {
        var created = CreateWithSwedishBranch(); // en v1 (RelatedContent=1), sv v1

        var swedishBefore = _fixture.Repository.Query<NewsContent>("sv").Where(x => x.Id == created.Id).First();
        Assert.Equal(new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent), swedishBefore.RelatedContent);

        // Change the shared property only on the master branch - sv is never touched.
        created.RelatedContent = new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent);
        _fixture.Repository.Update(created); // en v2

        // sv still has only its one version, but reading it now shows master's new
        // value: resolved live at read time, not copied at write time.
        var swedishAfter = _fixture.Repository.Query<NewsContent>("sv").Where(x => x.Id == created.Id).First();
        Assert.Equal(1, swedishAfter.VersionNumber); // sv itself was never versioned
        Assert.Equal(new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent), swedishAfter.RelatedContent);

        // History resolves the same live value too, even for that one sv version.
        var swedishHistory = _fixture.Repository.QueryHistory<NewsContent>(created.Id, "sv").Single();
        Assert.Equal(new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent), swedishHistory.RelatedContent);
    }

    [Fact]
    public void Non_master_branch_resolves_shared_values_from_the_masters_published_version_not_a_newer_unpublished_draft()
    {
        var created = CreateWithSwedishBranch();
        _fixture.Editing.Publish(created.Id, "en", 1, startPublish: null, stopPublish: null);

        created.RelatedContent = new ContentReference<ContentTypeKey>(2, ContentTypeKey.NewsContent);
        _fixture.Repository.Update(created); // en v2, an unpublished draft

        var swedish = _fixture.Repository.Query<NewsContent>("sv").Where(x => x.Id == created.Id).First();
        Assert.Equal(new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent), swedish.RelatedContent); // still v1, the published one
    }

    [Fact]
    public void Non_master_branch_falls_back_to_the_masters_latest_version_when_nothing_is_published()
    {
        var created = CreateWithSwedishBranch();
        // Nothing published on "en" at all - sv still resolves against en's latest draft.
        var swedish = _fixture.Repository.Query<NewsContent>("sv").Where(x => x.Id == created.Id).First();
        Assert.Equal(new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent), swedish.RelatedContent);
    }

    public void Dispose() => _fixture.Dispose();
}
