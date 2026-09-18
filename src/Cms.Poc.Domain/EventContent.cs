using Cms.Framework.Abstractions;

namespace Cms.Poc.Domain;

public class EventContent : Content
{
    [CultureSpecific]
    public string Title { get; set; } = string.Empty;

    [CultureSpecific]
    public string Description { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
}
