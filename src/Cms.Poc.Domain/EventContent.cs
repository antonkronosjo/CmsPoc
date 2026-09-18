using System.ComponentModel.DataAnnotations;
using Cms.Framework.Abstractions;

namespace Cms.Poc.Domain;

public class EventContent : Content
{
    [CultureSpecific]
    [ContentProperty(InputType.Text)]
    [Required]
    public string Title { get; set; } = string.Empty;

    [CultureSpecific]
    [ContentProperty(InputType.TextArea)]
    [Required]
    public string Description { get; set; } = string.Empty;

    [ContentProperty(InputType.DateTime)]
    public DateTime StartDate { get; set; }
}
