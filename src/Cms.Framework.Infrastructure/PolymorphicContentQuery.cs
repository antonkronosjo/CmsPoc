using System.Linq.Expressions;
using Cms.Framework.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Cms.Framework.Infrastructure;

/// <summary>
/// <see cref="IContentQuery{T}"/> for T = <see cref="Content"/> - queries
/// across every registered content type and preserves each result's
/// concrete runtime type. Executes in two phases, both real SQL:
/// <list type="number">
/// <item>Filter the shared <c>ContentRoots</c> table directly (predicates
/// only ever reference <see cref="Content"/>'s own members, which map 1:1
/// onto that table) to get matching (Id, ContentTypeKey) pairs.</item>
/// <item>Group matched ids by type and dispatch one id-scoped SQL query per
/// distinct type present, via each type's generated hydration path.</item>
/// </list>
/// Nothing here loads the whole content database into memory: phase 1 is a
/// single filtered query, phase 2's queries are each scoped to an
/// <c>Id IN (...)</c> list built from phase 1's results.
/// </summary>
internal sealed class PolymorphicContentQuery<TContentType> : IContentQuery<Content>
    where TContentType : struct, Enum
{
    private readonly CmsDbContext<TContentType> _db;
    private readonly IReadOnlyCollection<IContentTypeMetadata<TContentType>> _contentTypes;
    private readonly string _language;
    private readonly bool _publishedOnly;
    private IQueryable<ContentRoot<TContentType>> _rootQuery;

    public PolymorphicContentQuery(CmsDbContext<TContentType> db, IReadOnlyCollection<IContentTypeMetadata<TContentType>> contentTypes, string language, bool publishedOnly = false)
    {
        _db = db;
        _contentTypes = contentTypes;
        _language = language;
        _publishedOnly = publishedOnly;

        // Roots whose type is no longer registered (e.g. a content class that was
        // deleted) must be excluded in SQL: materializing their stored key would
        // throw, because it no longer maps to any enum member.
        var knownKeys = contentTypes.Select(m => m.ContentTypeKey).ToArray();
        _rootQuery = db.ContentRoots.Where(x => knownKeys.Contains(x.ContentTypeKey));
    }

    public IContentQuery<Content> Where(Expression<Func<Content, bool>> predicate)
    {
        var rewritten = new ContentRootPredicateRewriter<TContentType>().Rewrite(predicate);
        _rootQuery = _rootQuery.Where(rewritten);
        return this;
    }

    public IContentQuery<Content> OfTypes<TKey>(params TKey[] contentTypes) where TKey : struct, Enum
    {
        if (typeof(TKey) != typeof(TContentType))
            throw new ArgumentException($"Expected content types of '{typeof(TContentType).Name}' but got '{typeof(TKey).Name}'.");

        var keys = (TContentType[])(object)contentTypes;
        _rootQuery = _rootQuery.Where(x => keys.Contains(x.ContentTypeKey));
        return this;
    }

    public List<Content> ToList()
    {
        var matched = _rootQuery
            .OrderBy(x => x.Id)
            .Select(x => new MatchedRoot(x.Id, x.ContentTypeKey))
            .ToList();

        return Hydrate(matched);
    }

    public async Task<List<Content>> ToListAsync(CancellationToken cancellationToken = default)
    {
        var matched = await _rootQuery
            .OrderBy(x => x.Id)
            .Select(x => new MatchedRoot(x.Id, x.ContentTypeKey))
            .ToListAsync(cancellationToken);

        return Hydrate(matched);
    }

    public Content First() => ToList().First();
    public Content? FirstOrDefault() => ToList().FirstOrDefault();
    public async Task<Content> FirstAsync(CancellationToken cancellationToken = default) => (await ToListAsync(cancellationToken)).First();
    public async Task<Content?> FirstOrDefaultAsync(CancellationToken cancellationToken = default) => (await ToListAsync(cancellationToken)).FirstOrDefault();

    private List<Content> Hydrate(List<MatchedRoot> matched)
    {
        if (matched.Count == 0) return new List<Content>();

        var orderedIds = matched.Select(m => m.Id).ToList();
        var byId = new Dictionary<int, Content>();

        foreach (var group in matched.GroupBy(m => m.ContentTypeKey))
        {
            var metadata = _contentTypes.FirstOrDefault(m => EqualityComparer<TContentType>.Default.Equals(m.ContentTypeKey, group.Key));
            if (metadata is null) continue;

            var ids = group.Select(g => g.Id).ToList();
            foreach (var content in metadata.QueryByIds(_db, ids, _language, _publishedOnly))
            {
                byId[content.Id] = content;
            }
        }

        return orderedIds.Where(byId.ContainsKey).Select(id => byId[id]).ToList();
    }

    private sealed record MatchedRoot(int Id, TContentType ContentTypeKey);
}
