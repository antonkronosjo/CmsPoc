using Cms.Framework.Abstractions;
using Cms.Framework.Abstractions.Users;
using Cms.Framework.Infrastructure;
using Cms.Framework.Infrastructure.Editing;
using Cms.Framework.Sqlite;
using Cms.Poc.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Framework.Tests;

/// <summary>
/// Spins up a real, file-based SQLite database and wires the framework
/// exactly the way a real application would: register generated content
/// everything via the single <c>AddCms()</c> call (content types are
/// discovered from <c>[ContentType]</c>), and resolve <see cref="IContentRepository"/>
/// from DI. No test-only shortcuts around the framework's own entry points.
/// A new instance is created per test method (xUnit's default when a test
/// class owns one directly), so tests never see each other's data.
/// </summary>
public sealed class ContentTestFixture : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;
    private readonly string _dbPath;

    /// <param name="userAdapter">
    /// Registers this adapter, switching user tracking and role enforcement on.
    /// Leave <c>null</c> for the default, tracking-off setup.
    /// </param>
    public ContentTestFixture(ICmsUserAdapter? userAdapter = null)
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"cms-tests-{Guid.NewGuid():N}.db");

        var services = new ServiceCollection();
        // Pooling=False so the underlying file handle is released as soon as the
        // DbContext is disposed, letting each test clean up its own temp database.
        services.AddCms<ContentTypeKey>(cms =>
        {
            cms.UseSqlite($"Data Source={_dbPath};Pooling=False");
            if (userAdapter is not null)
                cms.ConfigureServices(s => s.AddSingleton(userAdapter));
        });
        _provider = services.BuildServiceProvider();

        _scope = _provider.CreateScope();
        Db = _scope.ServiceProvider.GetRequiredService<CmsDbContext<ContentTypeKey>>();
        Db.Database.EnsureCreated();
        Repository = _scope.ServiceProvider.GetRequiredService<IContentRepository>();
        Editing = _scope.ServiceProvider.GetRequiredService<IContentEditingService<ContentTypeKey>>();
    }

    public CmsDbContext<ContentTypeKey> Db { get; }
    public IContentRepository Repository { get; }
    public IContentEditingService<ContentTypeKey> Editing { get; }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }
}
