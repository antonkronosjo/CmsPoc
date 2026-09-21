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

        var collected = contentTypes.Collect().Combine(context.CompilationProvider.Select(static (c, _) => c.AssemblyName));

        context.RegisterSourceOutput(collected, static (spc, input) => Execute(spc, input.Left, input.Right));
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<ContentTypeModel> models, string? assemblyName)
    {
        var distinct = models.Distinct().OrderBy(m => m.FullyQualifiedName).ToList();
        var enumNamespace = ToNamespace(assemblyName);

        foreach (var model in distinct)
        {
            context.AddSource($"{model.ClassName}.Version.g.cs", CodeEmitter.EmitVersionEntity(model, enumNamespace));
            context.AddSource($"{model.ClassName}.Translation.g.cs", CodeEmitter.EmitTranslationEntity(model, enumNamespace));
            context.AddSource($"{model.ClassName}.Configurations.g.cs", CodeEmitter.EmitConfigurations(model));
            context.AddSource($"{model.ClassName}.Store.g.cs", CodeEmitter.EmitStore(model, enumNamespace));
        }

        if (distinct.Count == 0) return;

        context.AddSource("ContentTypeKey.g.cs", CodeEmitter.EmitContentTypeEnum(enumNamespace, distinct));
        context.AddSource("ContentFrameworkRegistration.g.cs", CodeEmitter.EmitRegistration(distinct, enumNamespace));
    }

    /// <summary>
    /// The enum lives in a namespace named after the assembly (with anything
    /// that is not a valid identifier character replaced), so two content
    /// assemblies never emit clashing public enums.
    /// </summary>
    private static string ToNamespace(string? assemblyName)
    {
        var name = string.IsNullOrEmpty(assemblyName) ? "Cms.Content" : assemblyName!;
        var chars = name.Select(c => char.IsLetterOrDigit(c) || c == '.' || c == '_' ? c : '_').ToArray();
        return new string(chars);
    }
}
