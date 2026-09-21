using System.Reflection;
using Cms.Framework.Abstractions;
using Cms.Framework.Infrastructure.Editing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Framework.Infrastructure;

public static class CmsServiceCollectionExtensions
{
    /// <summary>
    /// The single entry point for wiring up the CMS: registers every
    /// <c>[ContentType]</c> class found in the application's assemblies,
    /// <see cref="CmsDbContext{TContentType}"/>, <see cref="IContentRepository"/> and
    /// <see cref="IContentEditingService{TContentType}"/>.
    /// </summary>
    public static IServiceCollection AddCms<TContentType>(this IServiceCollection services, Action<CmsBuilder> configure)
        where TContentType : struct, Enum
    {
        var builder = new CmsBuilder();
        configure(builder);

        var databaseConfiguration = builder.DatabaseConfiguration
            ?? throw new InvalidOperationException(
                "No database configured. Call UseDatabase(...) (or a provider shortcut such as UseSqlite) inside AddCms.");

        var contentAssemblies = ContentAssemblyDiscovery.Find().Concat(builder.ExtraContentAssemblies).Distinct();
        foreach (var assembly in contentAssemblies)
        {
            foreach (var attribute in assembly.GetCustomAttributes<ContentTypeRegistrarAttribute>())
            {
                var registrar = (IContentTypeRegistrar)Activator.CreateInstance(attribute.RegistrarType)!;
                registrar.Register(services);
            }
        }

        services.AddDbContext<CmsDbContext<TContentType>>(databaseConfiguration);
        services.AddScoped<IContentRepository, ContentRepository<TContentType>>();
        services.AddScoped<IContentEditingService<TContentType>, ContentEditingService<TContentType>>();
        services.Configure<ContentRepositoryOptions>(o => o.DefaultLanguage = builder.DefaultLanguage);
        foreach (var registration in builder.ServiceRegistrations)
            registration(services);

        if (builder.EnsureDatabaseCreated && builder.MigrateDatabase)
            throw new InvalidOperationException(
                "EnsureDatabaseCreated and MigrateDatabase are mutually exclusive: EnsureCreated bypasses migrations.");

        if (builder.EnsureDatabaseCreated || builder.MigrateDatabase)
        {
            var migrate = builder.MigrateDatabase;
            services.AddHostedService(sp => new EnsureDatabaseCreatedService<TContentType>(sp, migrate));
        }

        return services;
    }
}
