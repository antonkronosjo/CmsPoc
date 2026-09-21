using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cms.Framework.Infrastructure;

internal sealed class EnsureDatabaseCreatedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly bool _migrate;

    public EnsureDatabaseCreatedService(IServiceProvider services, bool migrate)
    {
        _services = services;
        _migrate = migrate;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<CmsDbContext>().Database;
        if (_migrate)
            database.Migrate();
        else
            database.EnsureCreated();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
