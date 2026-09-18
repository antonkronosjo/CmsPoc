using Microsoft.Extensions.DependencyInjection;

namespace Cms.Framework.Infrastructure.Editing;

public static class ContentEditingServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IContentEditingService"/>. Call this alongside
    /// <c>AddCmsDbContext</c> and the generated <c>AddContentFramework()</c>
    /// extension, which register the <see cref="CmsDbContext"/>,
    /// <see cref="IContentRepository"/> and per-type metadata this service
    /// depends on.
    /// </summary>
    public static IServiceCollection AddContentEditing(this IServiceCollection services)
    {
        services.AddScoped<IContentEditingService, ContentEditingService>();
        return services;
    }
}
