using System.ComponentModel.DataAnnotations;
using Cms.Framework.Abstractions;

namespace Cms.Poc.Domain;

[ContentType]
public class TestContent : Content
{
    [CultureSpecific]
    [ContentProperty(InputType.Text)]
    [Required]
    public required string Heading { get; set; }

    [CultureSpecific]
    [ContentProperty(InputType.TextArea)]
    [Required]
    public required string Body { get; set; }

    [CultureSpecific]
    [ContentProperty(InputType.TextArea)]
    [Required]
    public required string TestProperty { get; set; }

    /// <summary>
    /// Id of another content item this article relates to, or null. Just a
    /// plain nullable int - "this is a reference" is entirely a frontend
    /// concern driven by <see cref="InputType.ContentReference"/>.
    /// </summary>
    [ContentProperty(InputType.ContentReference)]
    public int? RelatedContentId { get; set; }
}
