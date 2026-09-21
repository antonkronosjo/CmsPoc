using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cms.Framework.Infrastructure;

internal sealed class EnsureDatabaseCreatedService : IHostedService
{
    private readonly IServiceProvider _services;

    public EnsureDatabaseCreatedService(IServiceProvider services)
    {
        _services = services;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        scope.ServiceProvider.GetRequiredService<CmsDbContext>().Database.EnsureCreated();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
