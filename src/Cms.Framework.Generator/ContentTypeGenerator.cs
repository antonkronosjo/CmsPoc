using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Cms.Framework.Generator;

/// <summary>
/// Discovers every non-abstract class deriving from
/// <c>Cms.Framework.Abstractions.Content</c> and marked with
/// <c>[ContentType]</c> in the compilation and emits
/// its Root/Version/Translation persistence shape, EF Core configuration,
/// and DI registration. This is the only thing a developer needs to have
/// happen automatically for a new content type to "just work" - they write
/// the flat class, this generator does everything else, deterministically,
/// at compile time.
/// </summary>
[Generator]
public sealed class ContentTypeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var contentTypes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => ContentTypeDiscovery.IsCandidate(node),
                transform: (ctx, _) => ContentTypeDiscovery.TryGetContentTypeModel(ctx))
            .Where(model => model is not null)
            .Select((model, _) => model!);

        var collected = contentTypes.Collect();

        context.RegisterSourceOutput(collected, static (spc, models) => Execute(spc, models));
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<ContentTypeModel> models)
    {
        var distinct = models.Distinct().OrderBy(m => m.FullyQualifiedName).ToList();

        foreach (var model in distinct)
        {
            context.AddSource($"{model.ClassName}.Version.g.cs", CodeEmitter.EmitVersionEntity(model));
            context.AddSource($"{model.ClassName}.Translation.g.cs", CodeEmitter.EmitTranslationEntity(model));
            context.AddSource($"{model.ClassName}.Configurations.g.cs", CodeEmitter.EmitConfigurations(model));
            context.AddSource($"{model.ClassName}.Store.g.cs", CodeEmitter.EmitStore(model));
        }

        if (distinct.Count > 0)
            context.AddSource("ContentFrameworkRegistration.g.cs", CodeEmitter.EmitRegistration(distinct));
    }
}
