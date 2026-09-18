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
        List<PropertyModel> cultureSpecificProperties)
    {
        Namespace = @namespace;
        ClassName = className;
        FullyQualifiedName = fullyQualifiedName;
        InvariantProperties = invariantProperties;
        CultureSpecificProperties = cultureSpecificProperties;
    }

    public string Namespace { get; }
    public string ClassName { get; }
    public string FullyQualifiedName { get; }
    public List<PropertyModel> InvariantProperties { get; }
    public List<PropertyModel> CultureSpecificProperties { get; }

    public string VersionTypeName => ClassName + "Version";
    public string TranslationTypeName => ClassName + "Translation";
    public string StoreTypeName => ClassName + "Store";
    public string VersionTableName => ClassName + "Versions";
    public string TranslationTableName => ClassName + "Translations";

    public override bool Equals(object? obj)
        => obj is ContentTypeModel other
           && other.FullyQualifiedName == FullyQualifiedName
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
