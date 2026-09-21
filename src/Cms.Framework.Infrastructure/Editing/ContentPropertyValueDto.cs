using Cms.Framework.Abstractions;

namespace Cms.Framework.Infrastructure.Editing;

/// <summary>
/// Both the metadata a form needs to render a property (which template,
/// whether it's required) and the property's actual value - the same shape
/// flows from "here's the schema to build a form from" to "here's what the
/// user typed," so the frontend never needs a second, parallel type for
/// values versus descriptors.
/// </summary>
public sealed class ContentPropertyValueDto
{
    public required InputType InputType { get; set; }
    public required bool Required { get; set; }

    /// <summary>
    /// <c>true</c> when the value differs per language. Everything else is
    /// shared by all languages and only editable in the item's master language.
    /// </summary>
    public bool CultureSpecific { get; set; }
    public object? Value { get; set; }
}
