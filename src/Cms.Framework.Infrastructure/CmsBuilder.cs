using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Cms.Framework.Infrastructure;

/// <summary>Configuration passed to <c>AddCms</c>.</summary>
public sealed class CmsBuilder
{
    private readonly List<Assembly> _extraContentAssemblies = new();

    internal Action<DbContextOptionsBuilder>? DatabaseConfiguration { get; private set; }
    internal IReadOnlyList<Assembly> ExtraContentAssemblies => _extraContentAssemblies;

    /// <summary>Language used when a call doesn't specify one.</summary>
    public string DefaultLanguage { get; set; } = "en";

    /// <summary>
    /// When <c>true</c>, the schema is created on application start
    /// (<c>Database.EnsureCreated()</c>). This is not a migrations story.
    /// </summary>
    public bool EnsureDatabaseCreated { get; set; }

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
