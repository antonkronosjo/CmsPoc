using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cms.Framework.Generator;

/// <summary>
/// Builds the generated source text for one discovered content type, plus
/// the single aggregated DI-registration file for every discovered type.
/// Everything here is plain string templating over the compile-time-known
/// <see cref="ContentTypeModel"/> - no runtime reflection, fully deterministic.
/// </summary>
internal static class CodeEmitter
{
    private const string GeneratedNamespace = "Cms.Framework.Generated";

    private static readonly HashSet<string> KnownValueTypes = new()
    {
        "int", "long", "short", "byte", "bool", "double", "float", "decimal",
        "System.DateTime", "System.DateTimeOffset", "System.Guid", "System.TimeSpan",
    };

    public static string EmitVersionEntity(ContentTypeModel model)
    {
        var sb = new StringBuilder();
        AppendHeader(sb);
        sb.AppendLine($"namespace {GeneratedNamespace};");
        sb.AppendLine();
        sb.AppendLine($"public sealed class {model.VersionTypeName}");
        sb.AppendLine("{");
        sb.AppendLine("    public int Id { get; set; }");
        sb.AppendLine("    public int RootId { get; set; }");
        sb.AppendLine("    public global::Cms.Framework.Infrastructure.ContentRoot Root { get; set; } = null!;");
        sb.AppendLine("    public int VersionNumber { get; set; }");
        sb.AppendLine("    public global::System.DateTime Created { get; set; }");
        sb.AppendLine("    public global::System.DateTime? StartPublish { get; set; }");
        sb.AppendLine("    public global::System.DateTime? StopPublish { get; set; }");
        foreach (var property in model.InvariantProperties)
        {
            sb.AppendLine($"    public {property.TypeName} {property.Name} {{ get; set; }}{DefaultValueSuffix(property.TypeName)}");
        }
        sb.AppendLine($"    public global::System.Collections.Generic.List<{model.TranslationTypeName}> Translations {{ get; set; }} = new();");
        sb.AppendLine("}");
        return sb.ToString();
    }

    public static string EmitTranslationEntity(ContentTypeModel model)
    {
        var sb = new StringBuilder();
        AppendHeader(sb);
        sb.AppendLine($"namespace {GeneratedNamespace};");
        sb.AppendLine();
        sb.AppendLine($"public sealed class {model.TranslationTypeName}");
        sb.AppendLine("{");
        sb.AppendLine("    public int Id { get; set; }");
        sb.AppendLine("    public int VersionId { get; set; }");
        sb.AppendLine($"    public {model.VersionTypeName} Version {{ get; set; }} = null!;");
        sb.AppendLine("    public string Language { get; set; } = string.Empty;");
        foreach (var property in model.CultureSpecificProperties)
        {
            sb.AppendLine($"    public {property.TypeName} {property.Name} {{ get; set; }}{DefaultValueSuffix(property.TypeName)}");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    public static string EmitConfigurations(ContentTypeModel model)
    {
        var sb = new StringBuilder();
        AppendHeader(sb);
        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore.Metadata.Builders;");
        sb.AppendLine();
        sb.AppendLine($"namespace {GeneratedNamespace};");
        sb.AppendLine();

        // Version configuration
        sb.AppendLine($"public sealed class {model.VersionTypeName}Configuration : IEntityTypeConfiguration<{model.VersionTypeName}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public void Configure(EntityTypeBuilder<{model.VersionTypeName}> builder)");
        sb.AppendLine("    {");
        sb.AppendLine($"        builder.ToTable(\"{model.VersionTableName}\");");
        sb.AppendLine("        builder.HasKey(x => x.Id);");
        sb.AppendLine("        builder.HasOne(x => x.Root).WithMany().HasForeignKey(x => x.RootId).OnDelete(DeleteBehavior.Cascade);");
        sb.AppendLine("        builder.HasIndex(x => new { x.RootId, x.VersionNumber }).IsUnique();");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        // Translation configuration
        sb.AppendLine($"public sealed class {model.TranslationTypeName}Configuration : IEntityTypeConfiguration<{model.TranslationTypeName}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public void Configure(EntityTypeBuilder<{model.TranslationTypeName}> builder)");
        sb.AppendLine("    {");
        sb.AppendLine($"        builder.ToTable(\"{model.TranslationTableName}\");");
        sb.AppendLine("        builder.HasKey(x => x.Id);");
        sb.AppendLine("        builder.HasOne(x => x.Version).WithMany(v => v.Translations).HasForeignKey(x => x.VersionId).OnDelete(DeleteBehavior.Cascade);");
        sb.AppendLine("        builder.HasIndex(x => new { x.VersionId, x.Language }).IsUnique();");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        // Flat read-model configuration (ToSqlQuery)
        var sql = BuildReadSql(model);
        sb.AppendLine($"public sealed class {model.ClassName}ReadConfiguration : IEntityTypeConfiguration<{model.FullyQualifiedName}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public void Configure(EntityTypeBuilder<{model.FullyQualifiedName}> builder)");
        sb.AppendLine("    {");
        sb.AppendLine("        builder.HasNoKey();");
        sb.AppendLine("        builder.ToSqlQuery(@\"" + sql.Replace("\"", "\"\"") + "\");");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    public static string EmitStore(ContentTypeModel model)
    {
        var sb = new StringBuilder();
        AppendHeader(sb);
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine("using Cms.Framework.Infrastructure;");
        sb.AppendLine();
        sb.AppendLine($"namespace {GeneratedNamespace};");
        sb.AppendLine();
        sb.AppendLine($"public sealed class {model.StoreTypeName} : global::Cms.Framework.Infrastructure.IContentTypeStore<{model.FullyQualifiedName}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public string ContentTypeKey => \"{model.ClassName}\";");
        sb.AppendLine();

        // Create
        sb.AppendLine($"    public {model.FullyQualifiedName} Create(CmsDbContext db, {model.FullyQualifiedName} content, string language)");
        sb.AppendLine("    {");
        sb.AppendLine("        var root = new global::Cms.Framework.Infrastructure.ContentRoot");
        sb.AppendLine("        {");
        sb.AppendLine("            Name = content.Name,");
        sb.AppendLine("            ContentTypeKey = ContentTypeKey,");
        sb.AppendLine("            Created = global::System.DateTime.UtcNow,");
        sb.AppendLine("        };");
        sb.AppendLine("        db.ContentRoots.Add(root);");
        sb.AppendLine();
        sb.AppendLine($"        var version = new {model.VersionTypeName}");
        sb.AppendLine("        {");
        sb.AppendLine("            Root = root,");
        sb.AppendLine("            VersionNumber = 1,");
        sb.AppendLine("            Created = root.Created,");
        foreach (var p in model.InvariantProperties)
            sb.AppendLine($"            {p.Name} = content.{p.Name},");
        sb.AppendLine("        };");
        sb.AppendLine($"        db.Set<{model.VersionTypeName}>().Add(version);");
        sb.AppendLine();
        sb.AppendLine($"        var translation = new {model.TranslationTypeName}");
        sb.AppendLine("        {");
        sb.AppendLine("            Version = version,");
        sb.AppendLine("            Language = language,");
        foreach (var p in model.CultureSpecificProperties)
            sb.AppendLine($"            {p.Name} = content.{p.Name},");
        sb.AppendLine("        };");
        sb.AppendLine($"        db.Set<{model.TranslationTypeName}>().Add(translation);");
        sb.AppendLine();
        sb.AppendLine("        db.SaveChanges();");
        sb.AppendLine();
        sb.AppendLine($"        return new {model.FullyQualifiedName}");
        sb.AppendLine("        {");
        sb.AppendLine("            Id = root.Id,");
        sb.AppendLine("            Name = root.Name,");
        sb.AppendLine("            Language = language,");
        sb.AppendLine("            VersionNumber = version.VersionNumber,");
        sb.AppendLine("            Created = version.Created,");
        foreach (var p in model.InvariantProperties)
            sb.AppendLine($"            {p.Name} = version.{p.Name},");
        foreach (var p in model.CultureSpecificProperties)
            sb.AppendLine($"            {p.Name} = translation.{p.Name},");
        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Update
        sb.AppendLine($"    public {model.FullyQualifiedName} Update(CmsDbContext db, {model.FullyQualifiedName} content)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var currentVersion = db.Set<{model.VersionTypeName}>()");
        sb.AppendLine("            .Where(v => v.RootId == content.Id)");
        sb.AppendLine("            .OrderByDescending(v => v.VersionNumber)");
        sb.AppendLine("            .Include(v => v.Translations)");
        sb.AppendLine("            .Include(v => v.Root)");
        sb.AppendLine("            .First();");
        sb.AppendLine();
        sb.AppendLine($"        var newVersion = new {model.VersionTypeName}");
        sb.AppendLine("        {");
        sb.AppendLine("            RootId = content.Id,");
        sb.AppendLine("            VersionNumber = currentVersion.VersionNumber + 1,");
        sb.AppendLine("            Created = global::System.DateTime.UtcNow,");
        foreach (var p in model.InvariantProperties)
            sb.AppendLine($"            {p.Name} = content.{p.Name},");
        sb.AppendLine("        };");
        sb.AppendLine($"        db.Set<{model.VersionTypeName}>().Add(newVersion);");
        sb.AppendLine();
        sb.AppendLine("        var updatedLanguageSeen = false;");
        sb.AppendLine("        foreach (var oldTranslation in currentVersion.Translations)");
        sb.AppendLine("        {");
        sb.AppendLine("            var isUpdatedLanguage = oldTranslation.Language == content.Language;");
        sb.AppendLine("            if (isUpdatedLanguage) updatedLanguageSeen = true;");
        sb.AppendLine($"            db.Set<{model.TranslationTypeName}>().Add(new {model.TranslationTypeName}");
        sb.AppendLine("            {");
        sb.AppendLine("                Version = newVersion,");
        sb.AppendLine("                Language = oldTranslation.Language,");
        foreach (var p in model.CultureSpecificProperties)
            sb.AppendLine($"                {p.Name} = isUpdatedLanguage ? content.{p.Name} : oldTranslation.{p.Name},");
        sb.AppendLine("            });");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        if (!updatedLanguageSeen)");
        sb.AppendLine("        {");
        sb.AppendLine($"            db.Set<{model.TranslationTypeName}>().Add(new {model.TranslationTypeName}");
        sb.AppendLine("            {");
        sb.AppendLine("                Version = newVersion,");
        sb.AppendLine("                Language = content.Language,");
        foreach (var p in model.CultureSpecificProperties)
            sb.AppendLine($"                {p.Name} = content.{p.Name},");
        sb.AppendLine("            });");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        db.SaveChanges();");
        sb.AppendLine();
        sb.AppendLine($"        return new {model.FullyQualifiedName}");
        sb.AppendLine("        {");
        sb.AppendLine("            Id = content.Id,");
        sb.AppendLine("            Name = currentVersion.Root.Name,");
        sb.AppendLine("            Language = content.Language,");
        sb.AppendLine("            VersionNumber = newVersion.VersionNumber,");
        sb.AppendLine("            Created = newVersion.Created,");
        foreach (var p in model.InvariantProperties)
            sb.AppendLine($"            {p.Name} = newVersion.{p.Name},");
        foreach (var p in model.CultureSpecificProperties)
            sb.AppendLine($"            {p.Name} = content.{p.Name},");
        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();

        // QueryCurrent
        sb.AppendLine($"    public global::System.Linq.IQueryable<{model.FullyQualifiedName}> QueryCurrent(CmsDbContext db, string language, bool publishedOnly = false)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (!publishedOnly)");
        sb.AppendLine($"            return db.Set<{model.FullyQualifiedName}>().Where(x => x.Language == language);");
        sb.AppendLine();
        sb.AppendLine("        var now = global::System.DateTime.UtcNow;");
        sb.AppendLine($"        var eligible = db.Set<{model.VersionTypeName}>()");
        sb.AppendLine("            .Where(v => v.StartPublish != null && v.StartPublish <= now && (v.StopPublish == null || v.StopPublish > now))");
        sb.AppendLine("            .Include(v => v.Translations)");
        sb.AppendLine("            .Include(v => v.Root)");
        sb.AppendLine("            .ToList();");
        sb.AppendLine();
        sb.AppendLine("        var live = eligible.GroupBy(v => v.RootId).Select(g => g.OrderByDescending(v => v.StartPublish).First());");
        sb.AppendLine();
        sb.AppendLine($"        var result = new global::System.Collections.Generic.List<{model.FullyQualifiedName}>();");
        sb.AppendLine("        foreach (var v in live)");
        sb.AppendLine("        {");
        sb.AppendLine("            var translation = v.Translations.FirstOrDefault(t => t.Language == language);");
        sb.AppendLine("            if (translation is null) continue;");
        sb.AppendLine($"            result.Add(new {model.FullyQualifiedName}");
        sb.AppendLine("            {");
        sb.AppendLine("                Id = v.Root.Id,");
        sb.AppendLine("                Name = v.Root.Name,");
        sb.AppendLine("                Language = language,");
        sb.AppendLine("                VersionNumber = v.VersionNumber,");
        sb.AppendLine("                Created = v.Created,");
        sb.AppendLine("                StartPublish = v.StartPublish,");
        sb.AppendLine("                StopPublish = v.StopPublish,");
        foreach (var p in model.InvariantProperties)
            sb.AppendLine($"                {p.Name} = v.{p.Name},");
        foreach (var p in model.CultureSpecificProperties)
            sb.AppendLine($"                {p.Name} = translation.{p.Name},");
        sb.AppendLine("            });");
        sb.AppendLine("        }");
        sb.AppendLine("        return result.AsQueryable();");
        sb.AppendLine("    }");
        sb.AppendLine();

        // QueryHistory
        sb.AppendLine($"    public global::System.Collections.Generic.IReadOnlyList<{model.FullyQualifiedName}> QueryHistory(CmsDbContext db, int id, string language)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var versions = db.Set<{model.VersionTypeName}>()");
        sb.AppendLine("            .Where(v => v.RootId == id)");
        sb.AppendLine("            .OrderByDescending(v => v.VersionNumber)");
        sb.AppendLine("            .Include(v => v.Translations)");
        sb.AppendLine("            .Include(v => v.Root)");
        sb.AppendLine("            .ToList();");
        sb.AppendLine();
        sb.AppendLine($"        var result = new global::System.Collections.Generic.List<{model.FullyQualifiedName}>();");
        sb.AppendLine("        foreach (var v in versions)");
        sb.AppendLine("        {");
        sb.AppendLine("            var translation = v.Translations.FirstOrDefault(t => t.Language == language);");
        sb.AppendLine("            if (translation is null) continue;");
        sb.AppendLine($"            result.Add(new {model.FullyQualifiedName}");
        sb.AppendLine("            {");
        sb.AppendLine("                Id = v.Root.Id,");
        sb.AppendLine("                Name = v.Root.Name,");
        sb.AppendLine("                Language = language,");
        sb.AppendLine("                VersionNumber = v.VersionNumber,");
        sb.AppendLine("                Created = v.Created,");
        sb.AppendLine("                StartPublish = v.StartPublish,");
        sb.AppendLine("                StopPublish = v.StopPublish,");
        foreach (var p in model.InvariantProperties)
            sb.AppendLine($"                {p.Name} = v.{p.Name},");
        foreach (var p in model.CultureSpecificProperties)
            sb.AppendLine($"                {p.Name} = translation.{p.Name},");
        sb.AppendLine("            });");
        sb.AppendLine("        }");
        sb.AppendLine("        return result;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // GetLivePublishedVersionNumber
        sb.AppendLine("    public int? GetLivePublishedVersionNumber(CmsDbContext db, int rootId)");
        sb.AppendLine("    {");
        sb.AppendLine("        var now = global::System.DateTime.UtcNow;");
        sb.AppendLine($"        return db.Set<{model.VersionTypeName}>()");
        sb.AppendLine("            .Where(v => v.RootId == rootId && v.StartPublish != null && v.StartPublish <= now && (v.StopPublish == null || v.StopPublish > now))");
        sb.AppendLine("            .OrderByDescending(v => v.StartPublish)");
        sb.AppendLine("            .Select(v => (int?)v.VersionNumber)");
        sb.AppendLine("            .FirstOrDefault();");
        sb.AppendLine("    }");
        sb.AppendLine();

        // VersionExists
        sb.AppendLine("    public bool VersionExists(CmsDbContext db, int rootId, int versionNumber)");
        sb.AppendLine($"        => db.Set<{model.VersionTypeName}>().Any(v => v.RootId == rootId && v.VersionNumber == versionNumber);");
        sb.AppendLine();

        // SetPublishSchedule
        sb.AppendLine("    public void SetPublishSchedule(CmsDbContext db, int rootId, int versionNumber, global::System.DateTime? startPublish, global::System.DateTime? stopPublish)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var version = db.Set<{model.VersionTypeName}>().Single(v => v.RootId == rootId && v.VersionNumber == versionNumber);");
        sb.AppendLine("        version.StartPublish = startPublish;");
        sb.AppendLine("        version.StopPublish = stopPublish;");
        sb.AppendLine("        db.SaveChanges();");
        sb.AppendLine("    }");
        sb.AppendLine();

        // StopActivePublish
        sb.AppendLine("    public bool StopActivePublish(CmsDbContext db, int rootId, global::System.DateTime stopAt)");
        sb.AppendLine("    {");
        sb.AppendLine("        var now = global::System.DateTime.UtcNow;");
        sb.AppendLine($"        var active = db.Set<{model.VersionTypeName}>()");
        sb.AppendLine("            .Where(v => v.RootId == rootId && v.StartPublish != null && v.StartPublish <= now && (v.StopPublish == null || v.StopPublish > now))");
        sb.AppendLine("            .OrderByDescending(v => v.StartPublish)");
        sb.AppendLine("            .FirstOrDefault();");
        sb.AppendLine("        if (active is null) return false;");
        sb.AppendLine("        active.StopPublish = stopAt;");
        sb.AppendLine("        db.SaveChanges();");
        sb.AppendLine("        return true;");
        sb.AppendLine("    }");

        sb.AppendLine("}");
        return sb.ToString();
    }

    public static string EmitRegistration(IReadOnlyList<ContentTypeModel> models)
    {
        var sb = new StringBuilder();
        AppendHeader(sb);
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Cms.Framework.Infrastructure;");
        sb.AppendLine();
        sb.AppendLine($"namespace {GeneratedNamespace};");
        sb.AppendLine();
        sb.AppendLine("public static class ContentFrameworkServiceCollectionExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Registers every content type discovered in this compilation - one");
        sb.AppendLine("    /// generated IContentTypeStore&lt;T&gt; and IContentTypeMetadata per type.");
        sb.AppendLine("    /// Nothing here is written by hand; recompiling after adding a new");
        sb.AppendLine("    /// Content-derived class regenerates this method to include it.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static IServiceCollection AddContentFramework(this IServiceCollection services)");
        sb.AppendLine("    {");
        foreach (var model in models)
        {
            sb.AppendLine($"        services.AddSingleton<IContentTypeStore<{model.FullyQualifiedName}>, {model.StoreTypeName}>();");
            sb.AppendLine($"        services.AddSingleton<IContentTypeMetadata, ContentTypeMetadata<{model.FullyQualifiedName}>>();");
        }
        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildReadSql(ContentTypeModel model)
    {
        var invariantColumns = string.Join(string.Empty, model.InvariantProperties.Select(p => $",\n    v.{p.Name} AS {p.Name}"));
        var cultureColumns = string.Join(string.Empty, model.CultureSpecificProperties.Select(p => $",\n    t.{p.Name} AS {p.Name}"));

        return
$@"SELECT
    r.Id AS Id,
    r.Name AS Name,
    t.Language AS Language,
    v.VersionNumber AS VersionNumber,
    v.Created AS Created,
    v.StartPublish AS StartPublish,
    v.StopPublish AS StopPublish{invariantColumns}{cultureColumns}
FROM ContentRoots r
JOIN {model.VersionTableName} v ON v.RootId = r.Id
    AND v.VersionNumber = (SELECT MAX(v2.VersionNumber) FROM {model.VersionTableName} v2 WHERE v2.RootId = r.Id)
JOIN {model.TranslationTableName} t ON t.VersionId = v.Id
WHERE r.ContentTypeKey = '{model.ClassName}'";
    }

    private static string DefaultValueSuffix(string typeName)
    {
        if (typeName == "string") return " = string.Empty;";
        if (typeName.EndsWith("?", StringComparison.Ordinal)) return " = default;";
        if (KnownValueTypes.Contains(typeName)) return string.Empty;
        return " = default!;";
    }

    private static void AppendHeader(StringBuilder sb)
    {
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("// Generated by Cms.Framework.Generator - do not edit by hand.");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
    }
}
