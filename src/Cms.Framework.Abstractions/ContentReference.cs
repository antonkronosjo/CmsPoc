namespace Cms.Framework.Abstractions;

/// <summary>
/// A typed pointer to another content item: the item's globally unique
/// <see cref="Content.Id"/> plus its content type, expressed as the
/// <typeparamref name="TContentType"/> enum the source generator emits for the
/// application (e.g. <c>ContentTypeKey.NewsContent</c>). Use this - not a bare
/// <c>int</c> - for every property that refers to other content. Persisted as
/// a single <c>"ContentType:Id"</c> text column, with the enum stored by name.
/// </summary>
public readonly record struct ContentReference<TContentType>(int Id, TContentType ContentType)
    where TContentType : struct, Enum
{
    public override string ToString() => $"{ContentType}:{Id}";

    public static ContentReference<TContentType> Parse(string value)
        => TryParse(value, out var result)
            ? result
            : throw new FormatException($"'{value}' is not a valid content reference. Expected 'ContentType:Id'.");

    public static bool TryParse(string? value, out ContentReference<TContentType> result)
    {
        result = default;
        if (value is null) return false;

        var separator = value.LastIndexOf(':');
        if (separator <= 0 || !int.TryParse(value.AsSpan(separator + 1), out var id)) return false;

        var name = value[..separator];
        if (!Enum.TryParse<TContentType>(name, out var contentType) || contentType.ToString() != name) return false;

        result = new ContentReference<TContentType>(id, contentType);
        return true;
    }
}
