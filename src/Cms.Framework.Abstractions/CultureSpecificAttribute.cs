namespace Cms.Framework.Abstractions;

/// <summary>
/// Marks a property on a <see cref="Content"/>-derived class as culture
/// specific. Culture-specific properties are versioned per language branch.
/// Every other property is language-invariant: stored only on the master
/// language's branch, and resolved live from there - never copied - for
/// every other language.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class CultureSpecificAttribute : Attribute
{
}
