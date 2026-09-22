using Cms.Framework.Abstractions;
using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>Each language is its own branch: updating one must not create versions in, or otherwise disturb, the others.</summary>
public sealed class SingleLanguageUpdateTests : IDisposable
{
    private readonly ContentTestFixture _fixture = new();

    [Fact]
    public void Updating_one_language_leaves_other_languages_branches_untouched()
    {
        var created = _fixture.Repository.Create(new NewsContent
        {
            Name = "HEJ",
            Heading = "Hello",
            Body = "World",
            RelatedContent = new ContentReference<ContentTypeKey>(1, ContentTypeKey.NewsContent),
        }, "en");

        // add a Swedish translation as its own update
        var withSwedish = new NewsContent
        {
            Id = created.Id,
            Name = created.Name,
            Language = "sv",
            RelatedContent = created.RelatedContent,
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

        Assert.Equal(2, currentEnglish.VersionNumber);
        Assert.Equal("Hello (updated)", currentEnglish.Heading);

        Assert.Equal(1, currentSwedish.VersionNumber); // Swedish has its own numbering and was not touched
        Assert.Equal("Hej", currentSwedish.Heading);
        Assert.Equal("Varlden", currentSwedish.Body);
    }

    public void Dispose() => _fixture.Dispose();
}
