using System.ComponentModel.DataAnnotations;
using Cms.Framework.Abstractions;

namespace Cms.Poc.Domain;

[ContentType]
public class SpecialNewsContent : NewsContent
{

    [CultureSpecific]
    [ContentProperty(InputType.TextArea)]
    [Required]
    public string SpecialBody { get; set; } = string.Empty;
}