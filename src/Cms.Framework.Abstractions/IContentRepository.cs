namespace Cms.Framework.Abstractions;

/// <summary>
/// The only type the service/application layer talks to. Never references
/// generated root/version/translation entities - everything below this
/// interface (version creation, translation copy-forward, language
/// selection, joins) is handled internally.
/// </summary>
public interface IContentRepository
{
    /// <summary>Creates a new content instance (root + version 1 + translation) in the given language.</summary>
    T Create<T>(T content, string language) where T : Content;

    /// <summary>
    /// Creates a new, immutable version from <paramref name="content"/>. The
    /// language being changed is <see cref="Content.Language"/> on the
    /// passed-in instance; every other language's translation from the
    /// current version is copied forward unchanged.
    /// </summary>
    T Update<T>(T content) where T : Content;

    /// <summary>
    /// Queries content. When <typeparamref name="T"/> is exactly
    /// <see cref="Content"/>, this queries across every registered content
    /// type and preserves each result's concrete runtime type; predicates
    /// are then restricted to members declared on <see cref="Content"/>
    /// itself. When <typeparamref name="T"/> is a concrete content type,
    /// arbitrary predicates over its own flat properties are supported.
    /// </summary>
    /// <param name="language">The language to project translations in.</param>
    /// <param name="publishedOnly">
    /// When <c>false</c> (the default), returns the latest version of each
    /// matching content item regardless of publish state. When <c>true</c>,
    /// returns only the version currently live (per its publish window) for
    /// each item, omitting items with no version live right now.
    /// </param>
    IContentQuery<T> Query<T>(string? language = null, bool publishedOnly = false) where T : Content;

    /// <summary>Returns every version of a content instance, oldest first, in the given language.</summary>
    IReadOnlyList<T> QueryHistory<T>(int id, string? language = null) where T : Content;
}
