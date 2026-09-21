using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cms.Framework.Generator;

internal static class ContentTypeDiscovery
{
    private const string ContentTypeFullName = "Cms.Framework.Abstractions.Content";
    private const string ContentTypeAttributeFullName = "Cms.Framework.Abstractions.ContentTypeAttribute";
    private const string CultureSpecificAttributeFullName = "Cms.Framework.Abstractions.CultureSpecificAttribute";

    /// <summary>Names declared on <see cref="Cms.Framework.Abstractions.Content"/> itself - never re-emitted as Version/Translation columns.</summary>
    private static readonly HashSet<string> BaseContentMemberNames = new()
    {
        "Id", "Name", "Language", "VersionNumber", "Created",
    };

    public static bool IsCandidate(SyntaxNode node)
        => node is ClassDeclarationSyntax { BaseList.Types.Count: > 0, AttributeLists.Count: > 0 };

    public static ContentTypeModel? TryGetContentTypeModel(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
            return null;

        if (classSymbol.IsAbstract)
            return null;

        var contentTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName(ContentTypeFullName);
        if (contentTypeSymbol is null)
            return null;

        if (!DerivesFrom(classSymbol, contentTypeSymbol))
            return null;

        var contentTypeAttributeSymbol = semanticModel.Compilation.GetTypeByMetadataName(ContentTypeAttributeFullName);
        if (contentTypeAttributeSymbol is null)
            return null;

        if (!classSymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, contentTypeAttributeSymbol)))
            return null;

        var cultureSpecificAttributeSymbol =semanticModel.Compilation.GetTypeByMetadataName(CultureSpecificAttributeFullName);

        var invariant = new List<PropertyModel>();
        var cultureSpecific = new List<PropertyModel>();

        foreach (var member in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (member.IsStatic || member.IsIndexer) continue;
            if (member.DeclaredAccessibility != Accessibility.Public) continue;
            if (member.GetMethod is null || member.SetMethod is null) continue;
            if (BaseContentMemberNames.Contains(member.Name)) continue;

            var typeName = member.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (typeName.StartsWith("global::", System.StringComparison.Ordinal))
                typeName = typeName.Substring("global::".Length);
            var property = new PropertyModel(member.Name, typeName);

            var isCultureSpecific = cultureSpecificAttributeSymbol is not null
                && member.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, cultureSpecificAttributeSymbol));

            if (isCultureSpecific) cultureSpecific.Add(property);
            else invariant.Add(property);
        }

        var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : classSymbol.ContainingNamespace.ToDisplayString();

        var fullyQualifiedName = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return new ContentTypeModel(namespaceName, classSymbol.Name, fullyQualifiedName, invariant, cultureSpecific);
    }

    private static bool DerivesFrom(INamedTypeSymbol classSymbol, INamedTypeSymbol baseTypeSymbol)
    {
        var current = classSymbol.BaseType;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseTypeSymbol))
                return true;
            current = current.BaseType;
        }
        return false;
    }
}
