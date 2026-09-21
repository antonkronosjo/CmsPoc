namespace Cms.Framework.Abstractions;

/// <summary>
/// A typed pointer to another content item: the item's globally unique
/// <see cref="Content.Id"/> plus the key of its content type (the class name,
/// e.g. <c>"NewsContent"</c>). Use this - not a bare <c>int</c> - for every
/// property that refers to other content. Persisted as a single
/// <c>"ContentType:Id"</c> text column.
/// </summary>
public readonly record struct ContentReference(int Id, string ContentType)
{
    public override string ToString() => $"{ContentType}:{Id}";

    public static ContentReference Parse(string value)
        => TryParse(value, out var result)
            ? result
            : throw new FormatException($"'{value}' is not a valid content reference. Expected 'ContentType:Id'.");

    public static bool TryParse(string? value, out ContentReference result)
    {
        result = default;
        if (value is null) return false;

        var separator = value.LastIndexOf(':');
        if (separator <= 0 || !int.TryParse(value.AsSpan(separator + 1), out var id)) return false;

        result = new ContentReference(id, value[..separator]);
        return true;
    }
}
