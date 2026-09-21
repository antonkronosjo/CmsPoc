using Cms.Framework.Abstractions;
using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

public sealed class CreateContentTests : IDisposable
{
    private readonly ContentTestFixture _fixture = new();

    [Fact]
    public void Create_persists_root_version_1_and_a_translation()
    {
        var created = _fixture.Repository.Create(new NewsContent
        {
            Name = "HEJ",
            Heading = "Hello",
            Body = "World",
            RelatedContent = new ContentReference(1, "NewsContent"),
        }, language: "en");

        Assert.True(created.Id > 0);
        Assert.Equal("HEJ", created.Name);
        Assert.Equal(1, created.VersionNumber);
        Assert.Equal("en", created.Language);
        Assert.Equal(new ContentReference(1, "NewsContent"), created.RelatedContent);
        Assert.Equal("Hello", created.Heading);
        Assert.Equal("World", created.Body);

        Assert.Equal(1, _fixture.Db.ContentRoots.Count());
    }

    public void Dispose() => _fixture.Dispose();
}
