using Cms.Framework.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Cms.Framework.Sqlite;

public static class CmsBuilderSqliteExtensions
{
    /// <summary>Stores CMS content in SQLite.</summary>
    public static CmsBuilder UseSqlite(this CmsBuilder builder, string connectionString)
        => builder.UseDatabase(db => db.UseSqlite(connectionString));
}
