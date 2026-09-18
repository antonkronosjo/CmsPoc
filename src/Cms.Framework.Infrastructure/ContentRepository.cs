using Cms.Framework.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cms.Framework.Infrastructure;

/// <summary>
/// The generic repository the service layer talks to. Knows nothing about
/// any concrete content type at compile time - for concrete types it
/// resolves the generated <see cref="IContentTypeStore{T}"/> for
/// <typeparamref name="T"/> from DI (a normal, typed generic resolution,
/// not reflection-based scanning), and for the polymorphic base
/// <see cref="Content"/> type it delegates to <see cref="PolymorphicContentQuery"/>.
/// </summary>
internal sealed class ContentRepository : IContentRepository
{
    private readonly CmsDbContext _db;
    private readonly IServiceProvider _services;
    private readonly List<IContentTypeMetadata> _contentTypes;
    private readonly string _defaultLanguage;

    public ContentRepository(
        CmsDbContext db,
        IServiceProvider services,
        IEnumerable<IContentTypeMetadata> contentTypes,
        IOptions<ContentRepositoryOptions> options)
    {
        _db = db;
        _services = services;
        _contentTypes = contentTypes.ToList();
        _defaultLanguage = options.Value.DefaultLanguage;
    }

    public T Create<T>(T content, string language) where T : Content
        => GetStore<T>().Create(_db, content, language);

    public T Update<T>(T content) where T : Content
        => GetStore<T>().Update(_db, content);

    public IContentQuery<T> Query<T>(string? language = null) where T : Content
    {
        var lang = language ?? _defaultLanguage;

        if (typeof(T) == typeof(Content))
        {
            var polymorphic = new PolymorphicContentQuery(_db, _contentTypes, lang);
            return (IContentQuery<T>)(object)polymorphic;
        }

        return new EfBackedContentQuery<T>(GetStore<T>().QueryCurrent(_db, lang));
    }

    public IReadOnlyList<T> QueryHistory<T>(int id, string? language = null) where T : Content
        => GetStore<T>().QueryHistory(_db, id, language ?? _defaultLanguage);

    private IContentTypeStore<T> GetStore<T>() where T : Content
        => _services.GetRequiredService<IContentTypeStore<T>>();
}
