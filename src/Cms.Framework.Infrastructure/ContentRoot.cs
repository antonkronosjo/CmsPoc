namespace Cms.Framework.Infrastructure;

/// <summary>
/// The single, shared identity table for every content type. Hand-written
/// (not generated) because it carries no type-specific data - all
/// type-specific data lives in the generated per-type Version/Translation
/// tables. Because every content instance, regardless of type, gets its Id
/// from this one table's identity column, content Ids are globally unique
/// across types by construction - the database enforces it, not application code.
/// </summary>
/// <typeparam name="TContentType">The enum the source generator emits, one member per content type.</typeparam>
public class ContentRoot<TContentType> where TContentType : struct, Enum
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Discriminator identifying the concrete content type. Stored by name as text.</summary>
    public TContentType ContentTypeKey { get; set; }

    public DateTime Created { get; set; }

    /// <summary>
    /// The language this item was created in. Its <c>{Type}Version</c> branch
    /// is always in this language, and it is the only language in which the
    /// shared (non-culture-specific) properties and <see cref="Name"/> are
    /// editable; every other language is a <c>{Type}Translation</c> branch
    /// that resolves those values live from this one instead of storing them.
    /// </summary>
    public string MasterLanguage { get; set; } = string.Empty;
}
