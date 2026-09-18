using System.Linq.Expressions;
using Cms.Framework.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Cms.Framework.Infrastructure;

/// <summary>
/// <see cref="IContentQuery{T}"/> for a concrete content type, backed by a
/// real, composable EF Core <see cref="IQueryable{T}"/> (the generated
/// <c>ToSqlQuery</c>-mapped read entity). Every <see cref="Where"/> call and
/// every terminal call is translated to SQL by EF's own LINQ provider - no
/// custom expression handling needed here.
/// </summary>
internal sealed class EfBackedContentQuery<T> : IContentQuery<T> where T : Content
{
    private IQueryable<T> _source;

    public EfBackedContentQuery(IQueryable<T> source) => _source = source;

    public IContentQuery<T> Where(Expression<Func<T, bool>> predicate)
    {
        _source = _source.Where(predicate);
        return this;
    }

    public T First() => _source.First();
    public T? FirstOrDefault() => _source.FirstOrDefault();
    public List<T> ToList() => _source.ToList();

    public Task<T> FirstAsync(CancellationToken cancellationToken = default) => _source.FirstAsync(cancellationToken);
    public Task<T?> FirstOrDefaultAsync(CancellationToken cancellationToken = default) => _source.FirstOrDefaultAsync(cancellationToken);
    public Task<List<T>> ToListAsync(CancellationToken cancellationToken = default) => _source.ToListAsync(cancellationToken);
}
