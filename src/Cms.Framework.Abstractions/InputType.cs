using System.Text.Json.Serialization;

namespace Cms.Framework.Abstractions;

/// <summary>
/// Identifies which editor template the frontend should render for a
/// <see cref="ContentPropertyAttribute"/>-decorated property. Adding a new
/// field type to the editing UI is: add a value here, add one case to the
/// frontend's <c>FormElementTemplate</c> switch. Nothing else in the
/// pipeline (source generator, persistence, API) needs to know about it.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InputType
{
    Text = 0,
    TextArea = 1,
    Number = 2,
    Date = 3,
    DateTime = 4,

    /// <summary>
    /// Renders as a searchable picker for another content item. Backed by a
    /// <see cref="ContentReference"/> property.
    /// </summary>
    ContentReference = 5,
}
