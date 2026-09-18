using Cms.Framework.Abstractions;

namespace Cms.Framework.Infrastructure;

/// <summary>
/// Implemented once per content type by generated code. Encapsulates the
/// mapping between a flat content type and its generated Root/Version/
/// Translation persistence shape - the only place that mapping exists.
/// </summary>
public interface IContentTypeStore<T> where T : Content
{
    /// <summary>The discriminator stored on <see cref="ContentRoot.ContentTypeKey"/> for this type.</summary>
    string ContentTypeKey { get; }

    T Create(CmsDbContext db, T content, string language);

    T Update(CmsDbContext db, T content);

    /// <summary>Current-version, per-language flat read query. Composable - translates to SQL.</summary>
    IQueryable<T> QueryCurrent(CmsDbContext db, string language);

    IReadOnlyList<T> QueryHistory(CmsDbContext db, int id, string language);
}
