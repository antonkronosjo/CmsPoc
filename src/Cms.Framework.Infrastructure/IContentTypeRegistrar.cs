using Microsoft.Extensions.DependencyInjection;

namespace Cms.Framework.Infrastructure;

/// <summary>
/// Registers the per-type stores and metadata for every <c>[ContentType]</c>
/// class in one assembly. Implemented by generated code, never by hand.
/// </summary>
public interface IContentTypeRegistrar
{
    void Register(IServiceCollection services);
}

/// <summary>
/// Assembly-level marker emitted by the source generator, pointing at that
/// assembly's <see cref="IContentTypeRegistrar"/>. <c>AddCms</c> scans for it
/// to find content types without the host naming any assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class ContentTypeRegistrarAttribute : Attribute
{
    public ContentTypeRegistrarAttribute(Type registrarType)
    {
        RegistrarType = registrarType;
    }

    public Type RegistrarType { get; }
}
