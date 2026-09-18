using Cms.Framework.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Framework.Infrastructure;

public static class CmsDbContextServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="CmsDbContext"/> (SQLite) and <see cref="IContentRepository"/>.
    /// Call this alongside each content-defining assembly's generated
    /// <c>AddContentFramework()</c> extension, which registers the
    /// per-type stores and metadata this context and repository rely on.
    /// </summary>
    public static IServiceCollection AddCmsDbContext(
        this IServiceCollection services,
        string sqliteConnectionString,
        Action<ContentRepositoryOptions>? configureOptions = null)
    {
        services.AddDbContext<CmsDbContext>(options => options.UseSqlite(sqliteConnectionString));
        services.AddScoped<IContentRepository, ContentRepository>();
        services.Configure<ContentRepositoryOptions>(configureOptions ?? (_ => { }));
        return services;
    }
}
