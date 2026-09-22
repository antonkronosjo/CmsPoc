using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Cms.Framework.Abstractions;

namespace Cms.Framework.Infrastructure;

internal sealed class ParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _from;
    private readonly ParameterExpression _to;

    public ParameterReplacer(ParameterExpression from, ParameterExpression to)
    {
        _from = from;
        _to = to;
    }

    protected override Expression VisitParameter(ParameterExpression node)
        => node == _from ? _to : base.VisitParameter(node);
}

/// <summary>
/// Type-erased view of a registered content type, used by the polymorphic
/// <c>Query&lt;Content&gt;()</c> path to dispatch a hydration query for a
/// specific type without the caller needing to know that type at compile time.
/// </summary>
public interface IContentTypeMetadata<TContentType> where TContentType : struct, Enum
{
    TContentType ContentTypeKey { get; }
    Type ClrType { get; }

    /// <summary>The hex color from <see cref="ContentTypeAttribute.Color"/>, or <c>null</c> if none was set.</summary>
    string? Color { get; }

    /// <summary>
    /// Hydrates full flat objects for the given root ids, in the given
    /// language. A real, pushed-down SQL query scoped to just these ids -
    /// never a full table scan (except when <paramref name="publishedOnly"/>
    /// is <c>true</c>, which resolves each root's live version in memory).
    /// </summary>
    IReadOnlyList<Content> QueryByIds(CmsDbContext<TContentType> db, IReadOnlyCollection<int> ids, string? language, bool publishedOnly = false);

    /// <summary>
    /// Runs this type's current-version query in <paramref name="language"/>,
    /// applying each predicate and returning the materialized items. Each
    /// predicate is a single-parameter boolean lambda over any base type of
    /// <see cref="ClrType"/> (including <see cref="ClrType"/> itself); it is
    /// re-targeted onto <see cref="ClrType"/> so it still translates to SQL.
    /// </summary>
    IReadOnlyList<Content> QueryCurrent(CmsDbContext<TContentType> db, string? language, bool publishedOnly, IReadOnlyList<LambdaExpression> predicates);

    /// <summary>
    /// Type-erased entry point for creating content when the concrete type
    /// is only known at runtime (e.g. by name, from an HTTP request).
    /// <paramref name="content"/> must be an instance of <see cref="ClrType"/>.
    /// </summary>
    Content Create(CmsDbContext<TContentType> db, Content content, string language, string? userId);

    /// <summary>
    /// Type-erased entry point for updating content when the concrete type
    /// is only known at runtime. <paramref name="content"/> must be an
    /// instance of <see cref="ClrType"/>.
    /// </summary>
    Content Update(CmsDbContext<TContentType> db, Content content, string? userId);

    /// <summary>
    /// Type-erased entry point for fetching version history when the
    /// concrete type is only known at runtime.
    /// </summary>
    IReadOnlyList<Content> QueryHistory(CmsDbContext<TContentType> db, int id, string? language);

    /// <summary>The languages that have a branch (the master language, or at least one translated version) for each given item.</summary>
    IReadOnlyDictionary<int, IReadOnlyList<string>> QueryLanguages(CmsDbContext<TContentType> db, IReadOnlyCollection<int> ids);

    /// <summary>Each given item's publish state per language branch - one <see cref="LanguageStatusDto"/> per language returned by <see cref="QueryLanguages"/>.</summary>
    IReadOnlyDictionary<int, IReadOnlyList<LanguageStatusDto>> QueryLanguageStatuses(CmsDbContext<TContentType> db, IReadOnlyCollection<int> ids);

    /// <summary>The most recent <see cref="Content.Created"/> across every version, in every language branch, of each given item.</summary>
    IReadOnlyDictionary<int, DateTime> QueryLastModified(CmsDbContext<TContentType> db, IReadOnlyCollection<int> ids);

    /// <summary>
    /// The version number for this root in <paramref name="language"/>. When <paramref name="publishedOnly"/> is
    /// <c>false</c>, this is the most recent version number. When <c>true</c>, this is whichever version is
    /// currently live (per its publish window), or <c>null</c> if none is.
    /// </summary>
    int? GetVersionNumber(CmsDbContext<TContentType> db, int rootId, string language, bool publishedOnly);

    /// <summary>Whether a version with this number exists for this root in <paramref name="language"/>.</summary>
    bool VersionExists(CmsDbContext<TContentType> db, int rootId, string language, int versionNumber);

    /// <summary>Sets (or replaces) the publish window for a specific version.</summary>
    void SetPublishSchedule(CmsDbContext<TContentType> db, int rootId, string language, int versionNumber, DateTime? startPublish, DateTime? stopPublish, string? userId);

    /// <summary>Stops whichever version is currently live for this root in <paramref name="language"/>. Returns <c>false</c> (no-op) if nothing is live.</summary>
    bool StopActivePublish(CmsDbContext<TContentType> db, int rootId, string language, DateTime stopAt, string? userId);

    /// <summary>Clears every reference to <paramref name="userId"/> on this type's versions.</summary>
    void RemoveUserReferences(CmsDbContext<TContentType> db, string userId);
}

/// <summary>
/// Generic, hand-written (not generated) adapter from a typed
/// <see cref="IContentTypeStore{T, TContentType}"/> to the type-erased <see cref="IContentTypeMetadata{TContentType}"/>.
/// Generated code only needs to register the typed store; this wrapper is
/// registered once per type alongside it.
/// </summary>
public sealed class ContentTypeMetadata<T, TContentType> : IContentTypeMetadata<TContentType>
    where T : Content
    where TContentType : struct, Enum
{
    private readonly IContentTypeStore<T, TContentType> _store;

    public ContentTypeMetadata(IContentTypeStore<T, TContentType> store)
    {
        _store = store;
        ContentTypeKey = store.ContentTypeKey;

        var color = typeof(T).GetCustomAttribute<ContentTypeAttribute>(inherit: false)?.Color;
        if (color is not null && !HexColor.IsMatch(color))
            throw new InvalidOperationException(
                $"[ContentType(Color = \"{color}\")] on {typeof(T).Name} is not a valid hex color; expected #RGB or #RRGGBB.");
        Color = color;
    }

    private static readonly Regex HexColor = new("^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$", RegexOptions.Compiled);

    public TContentType ContentTypeKey { get; }
    public Type ClrType => typeof(T);
    public string? Color { get; }

    public IReadOnlyList<Content> QueryByIds(CmsDbContext<TContentType> db, IReadOnlyCollection<int> ids, string? language, bool publishedOnly = false)
        => _store.QueryCurrent(db, language, publishedOnly)
            .Where(x => ids.Contains(x.Id))
            .ToList()
            .Cast<Content>()
            .ToList();

    public IReadOnlyList<Content> QueryCurrent(CmsDbContext<TContentType> db, string? language, bool publishedOnly, IReadOnlyList<LambdaExpression> predicates)
    {
        var query = _store.QueryCurrent(db, language, publishedOnly);
        foreach (var predicate in predicates)
        {
            var parameter = Expression.Parameter(typeof(T), predicate.Parameters[0].Name);
            var body = new ParameterReplacer(predicate.Parameters[0], parameter).Visit(predicate.Body);
            query = query.Where(Expression.Lambda<Func<T, bool>>(body, parameter));
        }
        return query.ToList().Cast<Content>().ToList();
    }

    public Content Create(CmsDbContext<TContentType> db, Content content, string language, string? userId)
        => _store.Create(db, (T)content, language, userId);

    public Content Update(CmsDbContext<TContentType> db, Content content, string? userId)
        => _store.Update(db, (T)content, userId);

    public IReadOnlyList<Content> QueryHistory(CmsDbContext<TContentType> db, int id, string? language)
        => _store.QueryHistory(db, id, language).Cast<Content>().ToList();

    public IReadOnlyDictionary<int, IReadOnlyList<string>> QueryLanguages(CmsDbContext<TContentType> db, IReadOnlyCollection<int> ids)
        => _store.QueryLanguages(db, ids);

    public IReadOnlyDictionary<int, IReadOnlyList<LanguageStatusDto>> QueryLanguageStatuses(CmsDbContext<TContentType> db, IReadOnlyCollection<int> ids)
        => _store.QueryLanguageStatuses(db, ids);

    public IReadOnlyDictionary<int, DateTime> QueryLastModified(CmsDbContext<TContentType> db, IReadOnlyCollection<int> ids)
        => _store.QueryLastModified(db, ids);

    public int? GetVersionNumber(CmsDbContext<TContentType> db, int rootId, string language, bool publishedOnly)
        => _store.GetVersionNumber(db, rootId, language, publishedOnly);

    public bool VersionExists(CmsDbContext<TContentType> db, int rootId, string language, int versionNumber)
        => _store.VersionExists(db, rootId, language, versionNumber);

    public void SetPublishSchedule(CmsDbContext<TContentType> db, int rootId, string language, int versionNumber, DateTime? startPublish, DateTime? stopPublish, string? userId)
        => _store.SetPublishSchedule(db, rootId, language, versionNumber, startPublish, stopPublish, userId);

    public bool StopActivePublish(CmsDbContext<TContentType> db, int rootId, string language, DateTime stopAt, string? userId)
        => _store.StopActivePublish(db, rootId, language, stopAt, userId);

    public void RemoveUserReferences(CmsDbContext<TContentType> db, string userId)
        => _store.RemoveUserReferences(db, userId);
}
