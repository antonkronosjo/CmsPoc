namespace Cms.Framework.Abstractions;

/// <summary>
/// Marks a property on a <see cref="Content"/>-derived class as visible to
/// the generic content-editing API and UI, and says which editor template
/// should render it. A property without this attribute is never exposed by
/// the editing schema - it stays a purely internal/computed field. This is
/// a pure metadata marker read via reflection at request time; the source
/// generator does not know about it and is unaffected by it, exactly like
/// <see cref="CultureSpecificAttribute"/> is invisible outside the
/// generator's own concerns.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ContentPropertyAttribute : Attribute
{
    public ContentPropertyAttribute(InputType inputType = InputType.Text)
    {
        InputType = inputType;
    }

    public InputType InputType { get; }
}
