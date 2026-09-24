using System.Collections.Generic;

namespace Cms.Framework.Generator;

internal readonly struct PropertyModel
{
    public PropertyModel(string name, string typeName)
    {
        Name = name;
        TypeName = typeName;
    }

    public string Name { get; }

    /// <summary>Fully qualified, e.g. "string" or "System.DateTime".</summary>
    public string TypeName { get; }
}

internal sealed class ContentTypeModel
{
    public ContentTypeModel(
        string @namespace,
        string className,
        string fullyQualifiedName,
        List<PropertyModel> invariantProperties,
        List<PropertyModel> cultureSpecificProperties,
        bool isVersioned,
        bool isPublishable)
    {
        Namespace = @namespace;
        ClassName = className;
        FullyQualifiedName = fullyQualifiedName;
        InvariantProperties = invariantProperties;
        CultureSpecificProperties = cultureSpecificProperties;
        IsVersioned = isVersioned;
        IsPublishable = isPublishable;
    }

    public string Namespace { get; }
    public string ClassName { get; }
    public string FullyQualifiedName { get; }
    public List<PropertyModel> InvariantProperties { get; }
    public List<PropertyModel> CultureSpecificProperties { get; }

    /// <summary><c>[ContentType(Versioned = ...)]</c>, default <c>true</c>. Changes emitted behavior only, never the storage shape.</summary>
    public bool IsVersioned { get; }

    /// <summary><c>[ContentType(Publishable = ...)]</c>, default <c>true</c>. Changes emitted behavior only, never the storage shape.</summary>
    public bool IsPublishable { get; }

    /// <summary>Whether reads and in-place writes resolve each branch to its effective (published-or-latest) version.</summary>
    public bool UsesEffectiveVersion => !IsVersioned || !IsPublishable;

    public string VersionTypeName => ClassName + "Version";
    public string TranslationTypeName => ClassName + "Translation";
    public string StoreTypeName => ClassName + "Store";
    public string VersionTableName => ClassName + "Versions";
    public string TranslationTableName => ClassName + "Translations";

    public override bool Equals(object? obj)
        => obj is ContentTypeModel other
           && other.FullyQualifiedName == FullyQualifiedName
           && other.IsVersioned == IsVersioned
           && other.IsPublishable == IsPublishable
           && PropertiesEqual(other.InvariantProperties, InvariantProperties)
           && PropertiesEqual(other.CultureSpecificProperties, CultureSpecificProperties);

    public override int GetHashCode() => FullyQualifiedName.GetHashCode();

    private static bool PropertiesEqual(List<PropertyModel> a, List<PropertyModel> b)
    {
        if (a.Count != b.Count) return false;
        for (var i = 0; i < a.Count; i++)
        {
            if (a[i].Name != b[i].Name || a[i].TypeName != b[i].TypeName) return false;
        }
        return true;
    }
}
