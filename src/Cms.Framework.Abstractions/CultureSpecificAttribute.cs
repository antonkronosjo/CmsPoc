namespace Cms.Framework.Abstractions;

/// <summary>
/// Marks a property on a <see cref="Content"/>-derived class as culture
/// specific. The source generator stores such properties per-language in a
/// generated translation table; every other property is treated as
/// language-invariant and stored once per version.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class CultureSpecificAttribute : Attribute
{
}
