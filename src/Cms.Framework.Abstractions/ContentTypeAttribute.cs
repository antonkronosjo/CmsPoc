namespace Cms.Framework.Abstractions;

/// <summary>
/// Opts a <see cref="Content"/>-derived class into CMS storage. The source
/// generator only emits persistence code for classes carrying this attribute,
/// and <c>AddCms</c> discovers the marked types automatically - no assembly
/// or type ever needs to be listed at startup.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ContentTypeAttribute : Attribute
{
    /// <summary>
    /// Optional hex color code (<c>#RGB</c> or <c>#RRGGBB</c>) identifying this
    /// content type wherever it is rendered, in both the CMS and the public
    /// view. When omitted, the UI falls back to a neutral color.
    /// </summary>
    public string? Color { get; set; }
}
