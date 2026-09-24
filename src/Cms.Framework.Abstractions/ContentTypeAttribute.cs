namespace Cms.Framework.Abstractions;

/// <summary>
/// Opts a <see cref="Content"/>-derived class into CMS storage. The source
/// generator only emits persistence code for classes carrying this attribute,
/// and <c>AddCms</c> discovers the marked types automatically - no assembly
/// or type ever needs to be listed at startup.
/// </summary>
/// <remarks>
/// <see cref="Versioned"/> and <see cref="Publishable"/> only change how the
/// type is written and resolved, never its storage shape - either can be
/// switched at any time without a migration. Whenever either is off, each
/// language branch resolves to its <em>effective version</em>: the currently
/// published one, or, with nothing published, the latest.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ContentTypeAttribute : Attribute
{
    /// <summary>
    /// Whether every save adds a new version (the default). When <c>false</c>,
    /// a save overwrites the branch's effective version in place; history
    /// already stored is kept, never deleted.
    /// </summary>
    public bool Versioned { get; set; } = true;

    /// <summary>
    /// Whether versions go live through an explicit publish (the default).
    /// When <c>false</c>, every save goes live immediately and publishing
    /// or unpublishing manually is rejected.
    /// </summary>
    public bool Publishable { get; set; } = true;
}
