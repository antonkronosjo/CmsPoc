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
    /// The content item this article relates to, or null.
    /// </summary>
    [ContentProperty(InputType.ContentReference)]
    public ContentReference<ContentTypeKey>? RelatedContent { get; set; }
}
