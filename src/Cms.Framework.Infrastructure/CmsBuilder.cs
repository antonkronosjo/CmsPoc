using System.Reflection;
using Cms.Framework.Abstractions.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Framework.Infrastructure;

/// <summary>Configuration passed to <c>AddCms</c>.</summary>
public sealed class CmsBuilder
{
    private readonly List<Assembly> _extraContentAssemblies = new();
    private readonly List<Action<IServiceCollection>> _serviceRegistrations = new();

    internal Action<DbContextOptionsBuilder>? DatabaseConfiguration { get; private set; }
    internal IReadOnlyList<Assembly> ExtraContentAssemblies => _extraContentAssemblies;
    internal IReadOnlyList<Action<IServiceCollection>> ServiceRegistrations => _serviceRegistrations;

    /// <summary>Language used when a call doesn't specify one.</summary>
    public string DefaultLanguage { get; set; } = "en";

    /// <summary>
    /// When <c>true</c>, the schema is created on application start
    /// (<c>Database.EnsureCreated()</c>). This is not a migrations story.
    /// </summary>
    public bool EnsureDatabaseCreated { get; set; }

    /// <summary>
    /// When <c>true</c>, pending EF Core migrations are applied on application
    /// start (<c>Database.Migrate()</c>). The migrations belong to the consuming
    /// app, so the provider must be told where they live (for example the
    /// <c>migrationsAssembly</c> argument of <c>UseSqlite</c>). Use instead of
    /// <see cref="EnsureDatabaseCreated"/>, not together with it.
    /// </summary>
    public bool MigrateDatabase { get; set; }

    /// <summary>
    /// Chooses the database provider, e.g. <c>UseDatabase(db => db.UseSqlite(cs))</c>
    /// or the <c>UseSqlite</c> shortcut from <c>Cms.Framework.Sqlite</c>.
    /// </summary>
    public CmsBuilder UseDatabase(Action<DbContextOptionsBuilder> configure)
    {
        DatabaseConfiguration = configure;
        return this;
    }

    /// <summary>
    /// Turns on user tracking: the CMS records who created and published each
    /// version and enforces <see cref="CmsRole"/>s, taking the acting user
    /// from <typeparamref name="TAdapter"/> (registered scoped). Optional -
    /// without it nothing is recorded and nothing is enforced.
    /// </summary>
    public CmsBuilder UseUserAdapter<TAdapter>() where TAdapter : class, ICmsUserAdapter
        => ConfigureServices(services => services.AddScoped<ICmsUserAdapter, TAdapter>());

    /// <summary>
    /// Lets an add-on (such as an adapter shortcut) register the extra services
    /// it depends on together with the CMS's own registrations.
    /// </summary>
    public CmsBuilder ConfigureServices(Action<IServiceCollection> configure)
    {
        _serviceRegistrations.Add(configure);
        return this;
    }

    /// <summary>
    /// Escape hatch for content assemblies automatic discovery can't see
    /// (for example plugins loaded at runtime). Not needed in the normal case:
    /// classes marked <c>[ContentType]</c> are found on their own.
    /// </summary>
    public CmsBuilder AddContentAssembly(Assembly assembly)
    {
        _extraContentAssemblies.Add(assembly);
        return this;
    }
}
