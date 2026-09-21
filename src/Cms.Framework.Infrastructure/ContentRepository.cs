using Cms.Framework.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cms.Framework.Infrastructure;

/// <summary>
/// The generic repository the service layer talks to. Knows nothing about
/// any concrete content type at compile time - for concrete types it
/// resolves the generated <see cref="IContentTypeStore{T, TContentType}"/> for
/// <typeparamref name="T"/> from DI (a normal, typed generic resolution,
/// not reflection-based scanning), and for the polymorphic base
/// <see cref="Content"/> type it delegates to <see cref="PolymorphicContentQuery"/>.
/// </summary>
internal sealed class ContentRepository<TContentType> : IContentRepository
    where TContentType : struct, Enum
{
    private readonly CmsDbContext<TContentType> _db;
    private readonly IServiceProvider _services;
    private readonly List<IContentTypeMetadata<TContentType>> _contentTypes;
    private readonly string _defaultLanguage;

    public ContentRepository(
        CmsDbContext<TContentType> db,
        IServiceProvider services,
        IEnumerable<IContentTypeMetadata<TContentType>> contentTypes,
        IOptions<ContentRepositoryOptions> options)
    {
        _db = db;
        _services = services;
        _contentTypes = contentTypes.ToList();
        _defaultLanguage = options.Value.DefaultLanguage;
    }

    // Create/Update dispatch on the runtime type so a SpecialNewsContent passed
    // as NewsContent is stored in SpecialNewsContent's tables.
    public T Create<T>(T content, string language) where T : Content
        => (T)ResolveByRuntimeType(content).Create(_db, content, language);

    public T Update<T>(T content) where T : Content
        => (T)ResolveByRuntimeType(content).Update(_db, content);

    public IContentQuery<T> Query<T>(string? language = null, bool publishedOnly = false) where T : Content
    {
        var lang = language ?? _defaultLanguage;

        if (typeof(T) == typeof(Content))
        {
            var polymorphic = new PolymorphicContentQuery<TContentType>(_db, _contentTypes, lang, publishedOnly);
            return (IContentQuery<T>)(object)polymorphic;
        }

        var hierarchy = _contentTypes.Where(m => typeof(T).IsAssignableFrom(m.ClrType)).ToList();
        if (hierarchy.Count > 1)
            return new HierarchyContentQuery<T, TContentType>(_db, hierarchy, lang, publishedOnly);

        return new EfBackedContentQuery<T>(GetStore<T>().QueryCurrent(_db, lang, publishedOnly));
    }

    public IReadOnlyList<T> QueryHistory<T>(int id, string? language = null) where T : Content
    {
        var lang = language ?? _defaultLanguage;

        if (!_contentTypes.Any(m => m.ClrType != typeof(T) && typeof(T).IsAssignableFrom(m.ClrType)))
            return GetStore<T>().QueryHistory(_db, id, lang);

        // T has derived types: the id may belong to any of them, so dispatch on the root's stored type.
        var key = _db.ContentRoots.Where(r => r.Id == id).Select(r => (TContentType?)r.ContentTypeKey).FirstOrDefault();
        var metadata = key is null
            ? null
            : _contentTypes.FirstOrDefault(m => EqualityComparer<TContentType>.Default.Equals(m.ContentTypeKey, key.Value));
        if (metadata is null || !typeof(T).IsAssignableFrom(metadata.ClrType))
            return Array.Empty<T>();

        return metadata.QueryHistory(_db, id, lang).Cast<T>().ToList();
    }

    private IContentTypeMetadata<TContentType> ResolveByRuntimeType(Content content)
        => _contentTypes.FirstOrDefault(m => m.ClrType == content.GetType())
            ?? throw new InvalidOperationException($"'{content.GetType().Name}' is not a registered content type.");

    private IContentTypeStore<T, TContentType> GetStore<T>() where T : Content
        => _services.GetRequiredService<IContentTypeStore<T, TContentType>>();
}
