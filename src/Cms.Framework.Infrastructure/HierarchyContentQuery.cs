using System.Linq.Expressions;
using Cms.Framework.Abstractions;

namespace Cms.Framework.Infrastructure;

/// <summary>
/// <see cref="IContentQuery{T}"/> for a content type that other registered
/// types derive from (e.g. <c>NewsContent</c> and <c>SpecialNewsContent</c>).
/// Each content type is stored in its own tables, so this runs one SQL query
/// per type in the hierarchy (the type itself plus every registered
/// descendant), then merges the results ordered by <see cref="Content.Id"/>.
/// Each result keeps its concrete runtime type.
/// </summary>
internal sealed class HierarchyContentQuery<T, TContentType> : IContentQuery<T>
    where T : Content
    where TContentType : struct, Enum
{
    private readonly CmsDbContext<TContentType> _db;
    private readonly IReadOnlyList<IContentTypeMetadata<TContentType>> _hierarchy;
    private readonly string _language;
    private readonly bool _publishedOnly;
    private readonly List<LambdaExpression> _predicates = new();

    public HierarchyContentQuery(
        CmsDbContext<TContentType> db,
        IReadOnlyList<IContentTypeMetadata<TContentType>> hierarchy,
        string language,
        bool publishedOnly)
    {
        _db = db;
        _hierarchy = hierarchy;
        _language = language;
        _publishedOnly = publishedOnly;
    }

    public IContentQuery<T> Where(Expression<Func<T, bool>> predicate)
    {
        _predicates.Add(predicate);
        return this;
    }

    public IContentQuery<T> OfTypes<TKey>(params TKey[] contentTypes) where TKey : struct, Enum
        => throw new NotSupportedException(
            $"OfTypes is only supported on Query<Content>(). Query<{typeof(T).Name}>() is already restricted to that type hierarchy.");

    public List<T> ToList()
        => _hierarchy
            .SelectMany(m => m.QueryCurrent(_db, _language, _publishedOnly, _predicates))
            .Cast<T>()
            .OrderBy(x => x.Id)
            .ToList();

    public T First() => ToList().First();
    public T? FirstOrDefault() => ToList().FirstOrDefault();

    public Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(ToList());

    public Task<T> FirstAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(First());

    public Task<T?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(FirstOrDefault());
}
