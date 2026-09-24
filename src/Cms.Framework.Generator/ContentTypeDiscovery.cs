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

    /// <summary>
    /// Names declared on <see cref="Cms.Framework.Abstractions.Content"/> itself - never
    /// discovered as an attribute-driven Version/Translation column. <c>Name</c> is still
    /// emitted onto every generated <c>{Type}Version</c>/<c>{Type}Translation</c> as a
    /// built-in culture-specific column (see <see cref="CodeEmitter"/>), just not through
    /// this attribute-scanning path since it isn't a per-type declared property.
    /// </summary>
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

        var contentTypeAttribute = classSymbol.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, contentTypeAttributeSymbol));
        if (contentTypeAttribute is null)
            return null;

        var isVersioned = GetNamedBool(contentTypeAttribute, "Versioned", defaultValue: true);
        var isPublishable = GetNamedBool(contentTypeAttribute, "Publishable", defaultValue: true);

        var cultureSpecificAttributeSymbol =semanticModel.Compilation.GetTypeByMetadataName(CultureSpecificAttributeFullName);

        var invariant = new List<PropertyModel>();
        var cultureSpecific = new List<PropertyModel>();

        // Walk from the type up to (excluding) Content, then process base-first so
        // inherited properties come before derived ones. Each concrete type gets its
        // own full set of tables, so inherited properties are stored per type.
        var chain = new List<INamedTypeSymbol>();
        for (INamedTypeSymbol? current = classSymbol;
             current is not null && !SymbolEqualityComparer.Default.Equals(current, contentTypeSymbol);
             current = current.BaseType)
        {
            chain.Add(current);
        }
        chain.Reverse();

        var seen = new HashSet<string>();
        foreach (var member in chain.SelectMany(t => t.GetMembers().OfType<IPropertySymbol>()))
        {
            if (member.IsStatic || member.IsIndexer) continue;
            if (member.DeclaredAccessibility != Accessibility.Public) continue;
            if (member.GetMethod is null || member.SetMethod is null) continue;
            if (BaseContentMemberNames.Contains(member.Name)) continue;
            if (!seen.Add(member.Name)) continue;

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

        return new ContentTypeModel(namespaceName, classSymbol.Name, fullyQualifiedName, invariant, cultureSpecific, isVersioned, isPublishable);
    }

    private static bool GetNamedBool(AttributeData attribute, string name, bool defaultValue)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (argument.Key == name && argument.Value.Value is bool value)
                return value;
        }
        return defaultValue;
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
