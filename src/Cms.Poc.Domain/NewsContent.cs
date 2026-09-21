using System.ComponentModel.DataAnnotations;
using Cms.Framework.Abstractions;

namespace Cms.Poc.Domain;

[ContentType]
public class NewsContent : Content
{
    [CultureSpecific]
    [ContentProperty(InputType.Text)]
    [Required]
    public string Heading { get; set; } = string.Empty;

    [CultureSpecific]
    [ContentProperty(InputType.TextArea)]
    [Required]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Id of another content item this article relates to, or null. Just a
    /// plain nullable int - "this is a reference" is entirely a frontend
    /// concern driven by <see cref="InputType.ContentReference"/>.
    /// </summary>
    [ContentProperty(InputType.ContentReference)]
    public int? RelatedContentId { get; set; }
}
