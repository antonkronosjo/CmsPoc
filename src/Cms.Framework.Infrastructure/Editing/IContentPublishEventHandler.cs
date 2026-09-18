namespace Cms.Framework.Infrastructure.Editing;

/// <summary>
/// Extension point for reacting to publish/unpublish actions (e.g.
/// notifications, cache invalidation, search re-indexing). No
/// implementations are registered by default - register one or more via DI
/// to observe these events. Handlers fire only on an explicit
/// <see cref="IContentEditingService.Publish"/>/<see cref="IContentEditingService.Unpublish"/>
/// call, not automatically when a scheduled publish window is reached with
/// no request in flight.
/// </summary>
public interface IContentPublishEventHandler
{
    void OnPublished(int id, int versionNumber, DateTime startPublish, DateTime? stopPublish);

    void OnUnpublished(int id, int versionNumber);
}
