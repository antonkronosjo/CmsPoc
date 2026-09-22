using System.Linq.Expressions;
using Cms.Framework.Abstractions;

namespace Cms.Framework.Infrastructure;

/// <summary>
/// Retargets a predicate written against the flat <see cref="Content"/> base
/// class onto <see cref="ContentRoot{TContentType}"/>, the real EF entity it is answered
/// against for <c>Query&lt;Content&gt;()</c>. Only <see cref="Content.Id"/>
/// and <see cref="Content.Created"/> are supported: they are the only
/// members that mean the same thing before a specific version/language has
/// been resolved. <see cref="Content.Name"/> is culture-specific and only
/// exists per language branch, and <see cref="Content.Language"/> and
/// <see cref="Content.VersionNumber"/> only exist on a hydrated,
/// concrete-type projection, so referencing any of them here is a
/// compile-time-valid but semantically meaningless query - rejected
/// explicitly rather than silently mismapped.
/// </summary>
internal sealed class ContentRootPredicateRewriter<TContentType> : ExpressionVisitor
    where TContentType : struct, Enum
{
    private static readonly HashSet<string> SupportedMembers = new()
    {
        nameof(Content.Id),
        nameof(Content.Created),
    };

    private readonly ParameterExpression _rootParameter = Expression.Parameter(typeof(ContentRoot<TContentType>), "x");
    private ParameterExpression? _contentParameter;

    public Expression<Func<ContentRoot<TContentType>, bool>> Rewrite(Expression<Func<Content, bool>> predicate)
    {
        _contentParameter = predicate.Parameters[0];
        var body = Visit(predicate.Body);
        return Expression.Lambda<Func<ContentRoot<TContentType>, bool>>(body, _rootParameter);
    }

    protected override Expression VisitParameter(ParameterExpression node)
        => node == _contentParameter ? _rootParameter : base.VisitParameter(node);

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression == _contentParameter)
        {
            if (!SupportedMembers.Contains(node.Member.Name))
            {
                throw new NotSupportedException(
                    $"Query<Content>() predicates can only reference Content.Id or Content.Created. " +
                    $"'{node.Member.Name}' only exists once content has been resolved to a specific version and " +
                    $"language - query the concrete content type instead, or filter after the results are materialized.");
            }

            var rootProperty = typeof(ContentRoot<TContentType>).GetProperty(node.Member.Name)!;
            return Expression.Property(_rootParameter, rootProperty);
        }

        return base.VisitMember(node);
    }
}
