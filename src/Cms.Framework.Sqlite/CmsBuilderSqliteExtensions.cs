using System.Reflection;
using Cms.Framework.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Cms.Framework.Sqlite;

public static class CmsBuilderSqliteExtensions
{
    /// <summary>
    /// Stores CMS content in SQLite. Pass <paramref name="migrationsAssembly"/>
    /// (normally the host app's assembly) when using EF Core migrations, since
    /// the model is assembled from the app's content types and the migrations
    /// therefore live with the app, not in the framework.
    /// </summary>
    public static CmsBuilder UseSqlite(this CmsBuilder builder, string connectionString, Assembly? migrationsAssembly = null)
        => builder.UseDatabase(db => db.UseSqlite(
            connectionString,
            o =>
            {
                if (migrationsAssembly is not null)
                    o.MigrationsAssembly(migrationsAssembly.GetName().Name);
            }));
}
