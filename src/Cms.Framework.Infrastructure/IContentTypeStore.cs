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

    /// <summary>Creates the item. <paramref name="userId"/> is the opaque id recorded as <see cref="Content.CreatedBy"/>, or <c>null</c> when user tracking is off.</summary>
    T Create(CmsDbContext<TContentType> db, T content, string language, string? userId);

    /// <summary>Adds a new version. <paramref name="userId"/> is recorded as that version's <see cref="Content.CreatedBy"/>.</summary>
    T Update(CmsDbContext<TContentType> db, T content, string? userId);

    /// <summary>
    /// Per-language flat read query. When <paramref name="publishedOnly"/> is
    /// <c>false</c> (the default), this is the latest version of each item -
    /// composable, translates to SQL. When <c>true</c>, this is whichever
    /// version is currently live (per its publish window) for each item,
    /// omitting items with none live right now.
    /// </summary>
    IQueryable<T> QueryCurrent(CmsDbContext<TContentType> db, string? language, bool publishedOnly = false);

    /// <summary>
    /// One language's view of every version of an item, newest first.
    /// <paramref name="language"/> <c>null</c> means the item's master language.
    /// </summary>
    IReadOnlyList<T> QueryHistory(CmsDbContext<TContentType> db, int id, string? language);

    /// <summary>The languages that have a branch (the master language, or at least one translated version) for each given item.</summary>
    IReadOnlyDictionary<int, IReadOnlyList<string>> QueryLanguages(CmsDbContext<TContentType> db, IReadOnlyCollection<int> ids);

    /// <summary>Each given item's publish state per language branch - one <see cref="LanguageStatusDto"/> per language returned by <see cref="QueryLanguages"/>.</summary>
    IReadOnlyDictionary<int, IReadOnlyList<LanguageStatusDto>> QueryLanguageStatuses(CmsDbContext<TContentType> db, IReadOnlyCollection<int> ids);

    /// <summary>The most recent <see cref="Content.Created"/> across every version, in every language branch, of each given item.</summary>
    IReadOnlyDictionary<int, DateTime> QueryLastModified(CmsDbContext<TContentType> db, IReadOnlyCollection<int> ids);

    /// <summary>The version number currently live for this root's branch in <paramref name="language"/>, or <c>null</c> if none is.</summary>
    int? GetLivePublishedVersionNumber(CmsDbContext<TContentType> db, int rootId, string language);

    /// <summary>Whether a version with this number exists for this root in <paramref name="language"/>.</summary>
    bool VersionExists(CmsDbContext<TContentType> db, int rootId, string language, int versionNumber);

    /// <summary>Sets (or replaces) the publish window for a specific version, recording <paramref name="userId"/> as <see cref="Content.PublishedBy"/>.</summary>
    void SetPublishSchedule(CmsDbContext<TContentType> db, int rootId, string language, int versionNumber, DateTime? startPublish, DateTime? stopPublish, string? userId);

    /// <summary>
    /// Stops whichever version is currently live for this root in <paramref name="language"/> by setting
    /// its <see cref="Content.StopPublish"/> to <paramref name="stopAt"/>.
    /// Returns <c>false</c> (no-op) if nothing is currently live.
    /// </summary>
    bool StopActivePublish(CmsDbContext<TContentType> db, int rootId, string language, DateTime stopAt, string? userId);

    /// <summary>
    /// Clears <see cref="Content.CreatedBy"/> and <see cref="Content.PublishedBy"/> on every
    /// version of this type that references <paramref name="userId"/> (GDPR unlinking).
    /// The versions themselves are kept.
    /// </summary>
    void RemoveUserReferences(CmsDbContext<TContentType> db, string userId);
}
