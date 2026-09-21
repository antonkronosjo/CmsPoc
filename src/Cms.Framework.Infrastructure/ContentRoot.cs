namespace Cms.Framework.Infrastructure;

/// <summary>
/// The single, shared identity table for every content type. Hand-written
/// (not generated) because it carries no type-specific data - all
/// type-specific data lives in the generated per-type Version/Translation
/// tables. Because every content instance, regardless of type, gets its Id
/// from this one table's identity column, content Ids are globally unique
/// across types by construction - the database enforces it, not application code.
/// </summary>
public class ContentRoot
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Discriminator identifying the concrete content type (e.g. "NewsContent").</summary>
    public string ContentTypeKey { get; set; } = string.Empty;

    public DateTime Created { get; set; }
}
