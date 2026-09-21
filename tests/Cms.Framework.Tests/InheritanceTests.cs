using Cms.Poc.Domain;

namespace Cms.Framework.Tests;

/// <summary>A content type deriving from another content type is stored in its own tables and is returned by base-type queries.</summary>
public sealed class InheritanceTests : IDisposable
{
    private readonly ContentTestFixture _fixture = new();

    [Fact]
    public void Derived_type_stores_inherited_and_own_properties()
    {
        var created = _fixture.Repository.Create(new SpecialNewsContent { Name = "S", Heading = "H", Body = "B", SpecialBody = "SB" }, "en");

        var loaded = _fixture.Repository.Query<SpecialNewsContent>("en").Where(x => x.Id == created.Id).First();

        Assert.Equal("H", loaded.Heading);
        Assert.Equal("B", loaded.Body);
        Assert.Equal("SB", loaded.SpecialBody);
    }

    [Fact]
    public void Base_type_query_returns_derived_items_with_their_runtime_type()
    {
        var plain = _fixture.Repository.Create(new NewsContent { Name = "P", Heading = "Hp", Body = "Bp" }, "en");
        var special = _fixture.Repository.Create(new SpecialNewsContent { Name = "S", Heading = "Hs", Body = "Bs", SpecialBody = "SB" }, "en");

        var all = _fixture.Repository.Query<NewsContent>("en").ToList();
        var filtered = _fixture.Repository.Query<NewsContent>("en").Where(x => x.Heading == "Hs").ToList();
        var onlySpecial = _fixture.Repository.Query<SpecialNewsContent>("en").ToList();

        Assert.Equal(new[] { plain.Id, special.Id }, all.Select(x => x.Id));
        Assert.IsType<NewsContent>(all[0]);
        Assert.IsType<SpecialNewsContent>(all[1]);
        Assert.Equal(special.Id, Assert.IsType<SpecialNewsContent>(Assert.Single(filtered)).Id);
        Assert.Equal(special.Id, Assert.Single(onlySpecial).Id);
    }

    [Fact]
    public void Base_type_history_and_update_work_for_derived_items()
    {
        var special = _fixture.Repository.Create(new SpecialNewsContent { Name = "S", Heading = "H", Body = "B", SpecialBody = "v1" }, "en");
        special.SpecialBody = "v2";
        NewsContent asBase = special;

        _fixture.Repository.Update(asBase);
        var history = _fixture.Repository.QueryHistory<NewsContent>(special.Id, "en");

        Assert.Equal(new[] { 2, 1 }, history.Select(x => x.VersionNumber));
        Assert.Equal("v2", Assert.IsType<SpecialNewsContent>(history[0]).SpecialBody);
    }

    public void Dispose() => _fixture.Dispose();
}
