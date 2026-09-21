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
    /// <see cref="CmsDbContext"/>, <see cref="IContentRepository"/> and
    /// <see cref="IContentEditingService"/>.
    /// </summary>
    public static IServiceCollection AddCms(this IServiceCollection services, Action<CmsBuilder> configure)
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

        services.AddDbContext<CmsDbContext>(databaseConfiguration);
        services.AddScoped<IContentRepository, ContentRepository>();
        services.AddScoped<IContentEditingService, ContentEditingService>();
        services.Configure<ContentRepositoryOptions>(o => o.DefaultLanguage = builder.DefaultLanguage);

        if (builder.EnsureDatabaseCreated)
            services.AddHostedService<EnsureDatabaseCreatedService>();

        return services;
    }
}
