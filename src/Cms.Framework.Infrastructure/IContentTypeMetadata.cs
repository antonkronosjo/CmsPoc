using Cms.Framework.Abstractions;

namespace Cms.Framework.Infrastructure;

/// <summary>
/// Type-erased view of a registered content type, used by the polymorphic
/// <c>Query&lt;Content&gt;()</c> path to dispatch a hydration query for a
/// specific type without the caller needing to know that type at compile time.
/// </summary>
public interface IContentTypeMetadata
{
    string ContentTypeKey { get; }
    Type ClrType { get; }

    /// <summary>
    /// Hydrates full flat objects for the given root ids, in the given
    /// language. A real, pushed-down SQL query scoped to just these ids -
    /// never a full table scan.
    /// </summary>
    IReadOnlyList<Content> QueryByIds(CmsDbContext db, IReadOnlyCollection<int> ids, string language);

    /// <summary>
    /// Type-erased entry point for creating content when the concrete type
    /// is only known at runtime (e.g. by name, from an HTTP request).
    /// <paramref name="content"/> must be an instance of <see cref="ClrType"/>.
    /// </summary>
    Content Create(CmsDbContext db, Content content, string language);

    /// <summary>
    /// Type-erased entry point for updating content when the concrete type
    /// is only known at runtime. <paramref name="content"/> must be an
    /// instance of <see cref="ClrType"/>.
    /// </summary>
    Content Update(CmsDbContext db, Content content);

    /// <summary>
    /// Type-erased entry point for fetching version history when the
    /// concrete type is only known at runtime.
    /// </summary>
    IReadOnlyList<Content> QueryHistory(CmsDbContext db, int id, string language);
}

/// <summary>
/// Generic, hand-written (not generated) adapter from a typed
/// <see cref="IContentTypeStore{T}"/> to the type-erased <see cref="IContentTypeMetadata"/>.
/// Generated code only needs to register the typed store; this wrapper is
/// registered once per type alongside it.
/// </summary>
public sealed class ContentTypeMetadata<T> : IContentTypeMetadata where T : Content
{
    private readonly IContentTypeStore<T> _store;

    public ContentTypeMetadata(IContentTypeStore<T> store)
    {
        _store = store;
        ContentTypeKey = store.ContentTypeKey;
    }

    public string ContentTypeKey { get; }
    public Type ClrType => typeof(T);

    public IReadOnlyList<Content> QueryByIds(CmsDbContext db, IReadOnlyCollection<int> ids, string language)
        => _store.QueryCurrent(db, language)
            .Where(x => ids.Contains(x.Id))
            .ToList()
            .Cast<Content>()
            .ToList();

    public Content Create(CmsDbContext db, Content content, string language)
        => _store.Create(db, (T)content, language);

    public Content Update(CmsDbContext db, Content content)
        => _store.Update(db, (T)content);

    public IReadOnlyList<Content> QueryHistory(CmsDbContext db, int id, string language)
        => _store.QueryHistory(db, id, language).Cast<Content>().ToList();
}
