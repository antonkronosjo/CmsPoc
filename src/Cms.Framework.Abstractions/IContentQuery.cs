using System.Linq.Expressions;

namespace Cms.Framework.Abstractions;

/// <summary>
/// A deliberately small, deferred query surface over flat content objects.
/// This intentionally is NOT <see cref="IQueryable{T}"/>: for a concrete
/// content type it is backed internally by a real, composable EF Core
/// <see cref="IQueryable{T}"/> (so predicates translate to SQL), while for
/// the polymorphic base <c>Content</c> type it is backed by a two-phase
/// executor that cannot support arbitrary LINQ composition across
/// heterogeneous underlying tables. Exposing one narrow interface for both
/// keeps the repository API consistent without pretending the base-type
/// query is as capable as a concrete-type query.
/// </summary>
public interface IContentQuery<T> where T : Content
{
    /// <summary>Adds a filter. Deferred - not executed until a terminal call below.</summary>
    IContentQuery<T> Where(Expression<Func<T, bool>> predicate);

    T First();
    T? FirstOrDefault();
    List<T> ToList();

    Task<T> FirstAsync(CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(CancellationToken cancellationToken = default);
    Task<List<T>> ToListAsync(CancellationToken cancellationToken = default);
}
