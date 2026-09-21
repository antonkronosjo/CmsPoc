using Cms.Framework.Abstractions;

namespace Cms.Framework.Infrastructure;

/// <summary>
/// Implemented once per content type by generated code. Encapsulates the
/// mapping between a flat content type and its generated Root/Version/
/// Translation persistence shape - the only place that mapping exists.
/// </summary>
public interface IContentTypeStore<T, TContentType>
    where T : Content
    where TContentType : struct, Enum
{
    /// <summary>The discriminator stored on <see cref="ContentRoot{TContentType}.ContentTypeKey"/> for this type.</summary>
    TContentType ContentTypeKey { get; }

    T Create(CmsDbContext<TContentType> db, T content, string language);

    T Update(CmsDbContext<TContentType> db, T content);

    /// <summary>
    /// Per-language flat read query. When <paramref name="publishedOnly"/> is
    /// <c>false</c> (the default), this is the latest version of each item -
    /// composable, translates to SQL. When <c>true</c>, this is whichever
    /// version is currently live (per its publish window) for each item,
    /// omitting items with none live right now.
    /// </summary>
    IQueryable<T> QueryCurrent(CmsDbContext<TContentType> db, string language, bool publishedOnly = false);

    IReadOnlyList<T> QueryHistory(CmsDbContext<TContentType> db, int id, string language);

    /// <summary>The version number currently live for this root, or <c>null</c> if none is.</summary>
    int? GetLivePublishedVersionNumber(CmsDbContext<TContentType> db, int rootId);

    /// <summary>Whether a version with this number exists for this root.</summary>
    bool VersionExists(CmsDbContext<TContentType> db, int rootId, int versionNumber);

    /// <summary>Sets (or replaces) the publish window for a specific version.</summary>
    void SetPublishSchedule(CmsDbContext<TContentType> db, int rootId, int versionNumber, DateTime? startPublish, DateTime? stopPublish);

    /// <summary>
    /// Stops whichever version is currently live for this root by setting
    /// its <see cref="Content.StopPublish"/> to <paramref name="stopAt"/>.
    /// Returns <c>false</c> (no-op) if nothing is currently live.
    /// </summary>
    bool StopActivePublish(CmsDbContext<TContentType> db, int rootId, DateTime stopAt);
}
