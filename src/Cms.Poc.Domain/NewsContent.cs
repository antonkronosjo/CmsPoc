using Cms.Framework.Abstractions;

namespace Cms.Poc.Domain;

public class NewsContent : Content
{
    [CultureSpecific]
    public string Heading { get; set; } = string.Empty;

    [CultureSpecific]
    public string Body { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;
}
