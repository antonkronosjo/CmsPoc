namespace Cms.Framework.Infrastructure.Editing;

/// <summary>
/// Thrown when publishing or unpublishing is requested for a content type declared
/// <c>[ContentType(Publishable = false)]</c> - every save of such a type is live already.
/// </summary>
public sealed class ContentTypeNotPublishableException : InvalidOperationException
{
    public ContentTypeNotPublishableException(string contentTypeKey)
        : base($"Content type '{contentTypeKey}' is not publishable; every save goes live immediately.")
    {
    }
}
