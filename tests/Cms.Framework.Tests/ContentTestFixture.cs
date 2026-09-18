using Cms.Framework.Abstractions;
using Cms.Framework.Generated;
using Cms.Framework.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Framework.Tests;

/// <summary>
/// Spins up a real, file-based SQLite database and wires the framework
/// exactly the way a real application would: register generated content
/// types via <c>AddContentFramework()</c>, register the DbContext via
/// <c>AddCmsDbContext()</c>, and resolve <see cref="IContentRepository"/>
/// from DI. No test-only shortcuts around the framework's own entry points.
/// A new instance is created per test method (xUnit's default when a test
/// class owns one directly), so tests never see each other's data.
/// </summary>
public sealed class ContentTestFixture : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;
    private readonly string _dbPath;

    public ContentTestFixture()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"cms-tests-{Guid.NewGuid():N}.db");

        var services = new ServiceCollection();
        services.AddContentFramework();
        // Pooling=False so the underlying file handle is released as soon as the
        // DbContext is disposed, letting each test clean up its own temp database.
        services.AddCmsDbContext($"Data Source={_dbPath};Pooling=False");
        _provider = services.BuildServiceProvider();

        _scope = _provider.CreateScope();
        Db = _scope.ServiceProvider.GetRequiredService<CmsDbContext>();
        Db.Database.EnsureCreated();
        Repository = _scope.ServiceProvider.GetRequiredService<IContentRepository>();
    }

    public CmsDbContext Db { get; }
    public IContentRepository Repository { get; }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }
}
