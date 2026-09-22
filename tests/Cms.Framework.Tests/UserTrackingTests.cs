using Cms.Framework.Abstractions;
using Cms.Framework.Abstractions.Users;
using Cms.Framework.Infrastructure.Editing;
using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>
/// The optional user-tracking feature: attribution of versions and publishing
/// to the current user, role enforcement, and GDPR-style removal of references.
/// </summary>
public sealed class UserTrackingTests : IDisposable
{
    private readonly FakeUserAdapter _users = new();
    private readonly ContentTestFixture _fixture;

    public UserTrackingTests()
    {
        _fixture = new ContentTestFixture(_users);
    }

    private static NewsContent NewNews() => new()
    {
        Name = "A",
        Heading = "H",
        Body = "B",
        RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent),
    };

    private static CreateContentSchema<ContentTypeKey> NewNewsRequest() => new()
    {
        Metadata = new CreateContentMetadata<ContentTypeKey> { ContentTypeKey = ContentTypeKey.NewsContent, Language = "en", Name = "A" },
        Properties = new Dictionary<string, ContentPropertyValueDto>(),
    };

    [Fact]
    public void Without_an_adapter_nothing_is_recorded_and_nothing_is_enforced()
    {
        using var untracked = new ContentTestFixture();

        var created = untracked.Repository.Create(NewNews(), "en");
        untracked.Editing.Publish(created.Id, "en", created.VersionNumber, startPublish: null, stopPublish: null);

        var version = untracked.Repository.QueryHistory<NewsContent>(created.Id, "en").Single();
        Assert.Null(version.CreatedBy);
        Assert.Null(version.PublishedBy);
    }

    [Fact]
    public void Create_and_each_update_record_the_user_who_made_that_version()
    {
        _users.SignIn("alice", CmsRole.Editor);
        var created = _fixture.Repository.Create(NewNews(), "en");

        _users.SignIn("bob", CmsRole.Editor);
        created.Heading = "Changed";
        _fixture.Repository.Update(created);

        var history = _fixture.Repository.QueryHistory<NewsContent>(created.Id, "en");
        Assert.Equal("bob", history.Single(v => v.VersionNumber == 2).CreatedBy);
        Assert.Equal("alice", history.Single(v => v.VersionNumber == 1).CreatedBy);
    }

    [Fact]
    public void Publish_records_the_publisher_on_the_published_version()
    {
        _users.SignIn("alice", CmsRole.Admin);
        var created = _fixture.Repository.Create(NewNews(), "en");

        _fixture.Editing.Publish(created.Id, "en", created.VersionNumber, startPublish: null, stopPublish: null);

        var published = _fixture.Repository.Query<NewsContent>("en", publishedOnly: true).Where(x => x.Id == created.Id).First();
        Assert.Equal("alice", published.PublishedBy);
    }

    [Fact]
    public void An_editor_can_create_and_update_but_not_publish_or_unpublish()
    {
        _users.SignIn("alice", CmsRole.Editor);

        var created = _fixture.Editing.Create(NewNewsRequest());

        Assert.Throws<CmsForbiddenException>(() => _fixture.Editing.Publish(created.Id, "en", created.VersionNumber, null, null));
        Assert.Throws<CmsForbiddenException>(() => _fixture.Editing.Unpublish(created.Id, "en"));
    }

    [Fact]
    public void An_admin_can_publish()
    {
        _users.SignIn("alice", CmsRole.Admin);

        var created = _fixture.Editing.Create(NewNewsRequest());
        _fixture.Editing.Publish(created.Id, "en", created.VersionNumber, startPublish: null, stopPublish: null);

        Assert.NotEmpty(_fixture.Repository.Query<Content>("en", publishedOnly: true).Where(x => x.Id == created.Id).ToList());
    }

    [Fact]
    public void An_anonymous_caller_is_rejected_by_the_editing_service()
    {
        _users.SignOut();

        Assert.Throws<CmsUnauthenticatedException>(() => _fixture.Editing.Create(NewNewsRequest()));
    }

    [Fact]
    public void History_resolves_display_names_and_flags_users_the_adapter_no_longer_knows()
    {
        _users.SignIn("alice", CmsRole.Editor);
        var created = _fixture.Repository.Create(NewNews(), "en");
        _users.SignIn("bob", CmsRole.Editor);
        created.Heading = "Changed";
        _fixture.Repository.Update(created);
        _users.SignIn("carol", CmsRole.Admin);

        // Alice is known, Bob has been erased from the consumer's user store.
        _users.Profiles["alice"] = "Alice A.";

        var history = _fixture.Editing.GetHistory(created.Id, "en");

        var v1 = history.Single(x => x.VersionNumber == 1).CreatedBy!;
        Assert.Equal("Alice A.", v1.DisplayName);
        Assert.False(v1.Removed);

        var v2 = history.Single(x => x.VersionNumber == 2).CreatedBy!;
        Assert.Equal("bob", v2.Id);
        Assert.Null(v2.DisplayName);
        Assert.True(v2.Removed);
    }

    [Fact]
    public void RemoveUserReferences_clears_the_user_from_every_content_type_but_keeps_the_versions()
    {
        _users.SignIn("alice", CmsRole.Admin);
        var news = _fixture.Repository.Create(NewNews(), "en");
        var special = _fixture.Repository.Create(new SpecialNewsContent
        {
            Name = "S", Heading = "H", Body = "B", SpecialBody = "SB",
            RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent),
        }, "en");
        _fixture.Editing.Publish(news.Id, "en", news.VersionNumber, startPublish: null, stopPublish: null);

        _users.SignIn("bob", CmsRole.Admin);
        _fixture.Editing.RemoveUserReferences("alice");

        var newsVersion = _fixture.Repository.QueryHistory<NewsContent>(news.Id, "en").Single();
        Assert.Null(newsVersion.CreatedBy);
        Assert.Null(newsVersion.PublishedBy);
        var specialVersion = _fixture.Repository.QueryHistory<SpecialNewsContent>(special.Id, "en").Single();
        Assert.Null(specialVersion.CreatedBy);
    }

    [Fact]
    public void RemoveUserReferences_requires_the_admin_role()
    {
        _users.SignIn("alice", CmsRole.Editor);

        Assert.Throws<CmsForbiddenException>(() => _fixture.Editing.RemoveUserReferences("alice"));
    }

    public void Dispose() => _fixture.Dispose();

    /// <summary>An in-memory user store whose current user tests switch at will.</summary>
    private sealed class FakeUserAdapter : ICmsUserAdapter
    {
        private CmsUserIdentity? _current;

        public Dictionary<string, string> Profiles { get; } = new();

        public void SignIn(string id, CmsRole role) => _current = new CmsUserIdentity(id, new HashSet<CmsRole> { role });

        public void SignOut() => _current = null;

        public CmsUserIdentity? GetCurrentUser() => _current;

        public IReadOnlyDictionary<string, CmsUserProfile> ResolveProfiles(IEnumerable<string> ids)
            => ids.Where(Profiles.ContainsKey).ToDictionary(id => id, id => new CmsUserProfile(id, Profiles[id]));
    }
}
