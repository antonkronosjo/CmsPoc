using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>Requirement #12: language-aware querying.</summary>
public sealed class LanguageQueryTests : IDisposable
{
    private readonly ContentTestFixture _fixture = new();

    [Fact]
    public void Query_in_different_languages_returns_different_translations_for_the_same_root()
    {
        var created = _fixture.Repository.Create(new NewsContent
        {
            Name = "HEJ",
            Heading = "Hello",
            Body = "World",
            RelatedContentId = 1,
        }, "en");

        _fixture.Repository.Update(new NewsContent
        {
            Id = created.Id,
            Name = created.Name,
            Language = "sv",
            RelatedContentId = created.RelatedContentId,
            Heading = "Hej",
            Body = "Varlden",
        });

        var english = _fixture.Repository.Query<NewsContent>("en").Where(x => x.Id == created.Id).First();
        var swedish = _fixture.Repository.Query<NewsContent>("sv").Where(x => x.Id == created.Id).First();

        Assert.Equal("Hello", english.Heading);
        Assert.Equal("Hej", swedish.Heading);
        Assert.Equal("en", english.Language);
        Assert.Equal("sv", swedish.Language);
    }

    public void Dispose() => _fixture.Dispose();
}
