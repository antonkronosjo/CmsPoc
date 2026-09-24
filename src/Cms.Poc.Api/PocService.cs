using Cms.Framework.Abstractions;
using Cms.Framework.Infrastructure.Editing;
using Cms.Poc.Domain;

namespace Cms.Poc.Api;

/// <summary>
/// Scratch service for trying out <see cref="IContentRepository"/> from application code.
/// </summary>
internal sealed class PocService
{
    private readonly IContentRepository _contentRepository;
    private readonly IContentEditingService<ContentTypeKey> _contentEditingService;

    public PocService(IContentRepository contentRepository, IContentEditingService<ContentTypeKey> contentEditingService)
    {
        _contentRepository = contentRepository;
        _contentEditingService = contentEditingService;
    }

    public async Task DoSomeStuff()
    {
        //Alla nyheter på svenska
        var newsOnSwedish = await _contentRepository
            .Query<NewsContent>("sv")
            .ToListAsync();

        //Alla evenemang som startar idag
        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);
        var eventsThatStartToday = await _contentRepository.Query<EventContent>("sv")
            .Where(x => x.StartDate >= todayStart && x.StartDate < tomorrowStart)
            .ToListAsync();
    }
}
