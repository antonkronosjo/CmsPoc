using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>Requirement #3: updating one language must not disturb the others.</summary>
public sealed class SingleLanguageUpdateTests : IDisposable
{
    private readonly ContentTestFixture _fixture = new();

    [Fact]
    public void Updating_one_language_leaves_other_languages_translations_intact_in_the_new_version()
    {
        var created = _fixture.Repository.Create(new NewsContent
        {
            Name = "HEJ",
            Heading = "Hello",
            Body = "World",
            RelatedContentId = 1,
        }, "en");

        // add a Swedish translation as its own update
        var withSwedish = new NewsContent
        {
            Id = created.Id,
            Name = created.Name,
            Language = "sv",
            RelatedContentId = created.RelatedContentId,
            Heading = "Hej",
            Body = "Varlden",
        };
        _fixture.Repository.Update(withSwedish);

        // now change only the English heading
        var englishV2 = _fixture.Repository.Query<NewsContent>("en").Where(x => x.Id == created.Id).First();
        englishV2.Heading = "Hello (updated)";
        _fixture.Repository.Update(englishV2);

        var currentEnglish = _fixture.Repository.Query<NewsContent>("en").Where(x => x.Id == created.Id).First();
        var currentSwedish = _fixture.Repository.Query<NewsContent>("sv").Where(x => x.Id == created.Id).First();

        Assert.Equal(3, currentEnglish.VersionNumber);
        Assert.Equal("Hello (updated)", currentEnglish.Heading);

        Assert.Equal(3, currentSwedish.VersionNumber); // same version row set, Swedish copied forward unchanged
        Assert.Equal("Hej", currentSwedish.Heading);
        Assert.Equal("Varlden", currentSwedish.Body);
    }

    public void Dispose() => _fixture.Dispose();
}
