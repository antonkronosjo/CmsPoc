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

    private const string ContentTypeEnumName = "ContentTypeKey";

    /// <summary>C# predicate (over a {Type}Version <c>v</c>) for a version whose publish window contains <c>now</c>.</summary>
    private const string MasterWindow = "v.StartPublish != null && v.StartPublish <= now && (v.StopPublish == null || v.StopPublish > now)";

    /// <summary>C# predicate (over a {Type}Translation <c>t</c>) for a version whose publish window contains <c>now</c>.</summary>
    private const string TranslationWindow = "t.StartPublish != null && t.StartPublish <= now && (t.StopPublish == null || t.StopPublish > now)";

    /// <summary>Fully qualified name of the enum <see cref="EmitContentTypeEnum"/> emits into <paramref name="enumNamespace"/>.</summary>
    public static string ContentTypeEnumFullName(string enumNamespace) => $"global::{enumNamespace}.{ContentTypeEnumName}";

    public static string EmitContentTypeEnum(string enumNamespace, IReadOnlyList<ContentTypeModel> models)
    {
        var sb = new StringBuilder();
        AppendHeader(sb);
        sb.AppendLine($"namespace {enumNamespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// One member per [ContentType] class in this assembly. Persisted by name, so");
        sb.AppendLine("/// adding, removing or reordering content types never changes what a stored value means.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("[global::System.Text.Json.Serialization.JsonConverter(typeof(global::System.Text.Json.Serialization.JsonStringEnumConverter))]");
        sb.AppendLine($"public enum {ContentTypeEnumName}");
        sb.AppendLine("{");
        foreach (var model in models)
            sb.AppendLine($"    {model.ClassName},");
        sb.AppendLine("}");
        return sb.ToString();
    }

    public static string EmitVersionEntity(ContentTypeModel model, string enumNamespace)
    {
        var contentTypeEnum = ContentTypeEnumFullName(enumNamespace);
        var sb = new StringBuilder();
        AppendHeader(sb);
        sb.AppendLine($"using {enumNamespace};");
        sb.AppendLine();
        sb.AppendLine($"namespace {GeneratedNamespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// One version of the item's master-language branch. Holds both the");
        sb.AppendLine("/// language-invariant properties - the single place they are stored,");
        sb.AppendLine("/// every non-master branch resolves them from here rather than storing");
        sb.AppendLine("/// its own copy - and the master language's own culture-specific text");
        sb.AppendLine("/// (including its own <c>Name</c>), versioned together as one history.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public sealed class {model.VersionTypeName}");
        sb.AppendLine("{");
        sb.AppendLine("    public int Id { get; set; }");
        sb.AppendLine("    public int RootId { get; set; }");
        sb.AppendLine($"    public global::Cms.Framework.Infrastructure.ContentRoot<{contentTypeEnum}> Root {{ get; set; }} = null!;");
        sb.AppendLine("    public string Name { get; set; } = string.Empty;");
        sb.AppendLine("    public int VersionNumber { get; set; }");
        sb.AppendLine("    public global::System.DateTime Created { get; set; }");
        sb.AppendLine("    public global::System.DateTime? StartPublish { get; set; }");
        sb.AppendLine("    public global::System.DateTime? StopPublish { get; set; }");
        sb.AppendLine("    public string? CreatedBy { get; set; }");
        sb.AppendLine("    public string? PublishedBy { get; set; }");
        foreach (var property in model.InvariantProperties.Concat(model.CultureSpecificProperties))
            sb.AppendLine($"    public {property.TypeName} {property.Name} {{ get; set; }}{DefaultValueSuffix(property.TypeName)}");
        sb.AppendLine("}");
        return sb.ToString();
    }

    public static string EmitTranslationEntity(ContentTypeModel model, string enumNamespace)
    {
        var contentTypeEnum = ContentTypeEnumFullName(enumNamespace);
        var sb = new StringBuilder();
        AppendHeader(sb);
        sb.AppendLine($"using {enumNamespace};");
        sb.AppendLine();
        sb.AppendLine($"namespace {GeneratedNamespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// One version of one non-master-language branch. Holds that language's");
        sb.AppendLine("/// own culture-specific text - including its own <c>Name</c>, stored and");
        sb.AppendLine("/// editable independently per language, never copied from or resolved");
        sb.AppendLine("/// against the master branch - plus its own version number and publish");
        sb.AppendLine("/// window. The language-invariant properties are never stored here; they");
        sb.AppendLine("/// are resolved live, at read time, from whichever version is currently");
        sb.AppendLine($"/// published (or, with nothing published, the latest version) on the");
        sb.AppendLine($"/// item's master-language <see cref=\"{model.VersionTypeName}\"/> branch, so those are never copied.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public sealed class {model.TranslationTypeName}");
        sb.AppendLine("{");
        sb.AppendLine("    public int Id { get; set; }");
        sb.AppendLine("    public int RootId { get; set; }");
        sb.AppendLine($"    public global::Cms.Framework.Infrastructure.ContentRoot<{contentTypeEnum}> Root {{ get; set; }} = null!;");
        sb.AppendLine("    public string Name { get; set; } = string.Empty;");
        sb.AppendLine("    public string Language { get; set; } = string.Empty;");
        sb.AppendLine("    public int VersionNumber { get; set; }");
        sb.AppendLine("    public global::System.DateTime Created { get; set; }");
        sb.AppendLine("    public global::System.DateTime? StartPublish { get; set; }");
        sb.AppendLine("    public global::System.DateTime? StopPublish { get; set; }");
        sb.AppendLine("    public string? CreatedBy { get; set; }");
        sb.AppendLine("    public string? PublishedBy { get; set; }");
        foreach (var property in model.CultureSpecificProperties)
            sb.AppendLine($"    public {property.TypeName} {property.Name} {{ get; set; }}{DefaultValueSuffix(property.TypeName)}");
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

        // Version configuration (master-language branch)
        sb.AppendLine($"public sealed class {model.VersionTypeName}Configuration : IEntityTypeConfiguration<{model.VersionTypeName}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public void Configure(EntityTypeBuilder<{model.VersionTypeName}> builder)");
        sb.AppendLine("    {");
        sb.AppendLine($"        builder.ToTable(\"{model.VersionTableName}\");");
        sb.AppendLine("        builder.HasKey(x => x.Id);");
        sb.AppendLine("        builder.HasOne(x => x.Root).WithMany().HasForeignKey(x => x.RootId).OnDelete(DeleteBehavior.Cascade);");
        sb.AppendLine("        builder.HasIndex(x => new { x.RootId, x.VersionNumber }).IsUnique();");
        sb.AppendLine("        builder.Property(x => x.Name).IsRequired();");
        sb.AppendLine("        builder.HasIndex(x => x.Name);");
        sb.AppendLine("        builder.Property(x => x.CreatedBy).HasMaxLength(256);");
        sb.AppendLine("        builder.Property(x => x.PublishedBy).HasMaxLength(256);");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        // Translation configuration (non-master-language branches)
        sb.AppendLine($"public sealed class {model.TranslationTypeName}Configuration : IEntityTypeConfiguration<{model.TranslationTypeName}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public void Configure(EntityTypeBuilder<{model.TranslationTypeName}> builder)");
        sb.AppendLine("    {");
        sb.AppendLine($"        builder.ToTable(\"{model.TranslationTableName}\");");
        sb.AppendLine("        builder.HasKey(x => x.Id);");
        sb.AppendLine("        builder.HasOne(x => x.Root).WithMany().HasForeignKey(x => x.RootId).OnDelete(DeleteBehavior.Cascade);");
        sb.AppendLine("        builder.HasIndex(x => new { x.RootId, x.Language, x.VersionNumber }).IsUnique();");
        sb.AppendLine("        builder.Property(x => x.Name).IsRequired();");
        sb.AppendLine("        builder.HasIndex(x => x.Name);");
        sb.AppendLine("        builder.Property(x => x.CreatedBy).HasMaxLength(256);");
        sb.AppendLine("        builder.Property(x => x.PublishedBy).HasMaxLength(256);");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        // Flat read-model configuration (ToSqlQuery)
        var sql = BuildReadSql(model);
        sb.AppendLine($"public sealed class {model.ClassName}ReadConfiguration : IEntityTypeConfiguration<{model.FullyQualifiedName}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public void Configure(EntityTypeBuilder<{model.FullyQualifiedName}> builder)");
        sb.AppendLine("    {");
        sb.AppendLine("        // Each content type is stored in its own tables; never let EF infer a CLR-inheritance hierarchy between read entities.");
        sb.AppendLine("        builder.HasBaseType((global::System.Type?)null);");
        sb.AppendLine("        builder.HasNoKey();");
        sb.AppendLine("        builder.ToSqlQuery(@\"" + sql.Replace("\"", "\"\"") + "\");");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    public static string EmitStore(ContentTypeModel model, string enumNamespace)
    {
        var contentTypeEnum = ContentTypeEnumFullName(enumNamespace);
        var V = model.VersionTypeName;
        var Tr = model.TranslationTypeName;
        var C = model.FullyQualifiedName;
        const string masterWindow = MasterWindow;
        const string translationWindow = TranslationWindow;

        // A branch's current row within a grouping `g` of its versions: the effective
        // (published, else latest) version while either flag is off, otherwise the latest.
        string BranchCurrentExpression(string x, string window) => model.UsesEffectiveVersion
            ? $"g.Where({x} => {window}).OrderByDescending({x} => {x}.StartPublish).FirstOrDefault() ?? g.OrderByDescending({x} => {x}.VersionNumber).First()"
            : $"g.OrderByDescending({x} => {x}.VersionNumber).First()";

        var sb = new StringBuilder();
        AppendHeader(sb);
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine("using Cms.Framework.Infrastructure;");
        sb.AppendLine();
        sb.AppendLine($"namespace {GeneratedNamespace};");
        sb.AppendLine();
        sb.AppendLine($"public sealed class {model.StoreTypeName} : global::Cms.Framework.Infrastructure.IContentTypeStore<{C}, {contentTypeEnum}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public {contentTypeEnum} ContentTypeKey => {contentTypeEnum}.{model.ClassName};");
        sb.AppendLine();
        sb.AppendLine($"    public bool IsVersioned => {(model.IsVersioned ? "true" : "false")};");
        sb.AppendLine();
        sb.AppendLine($"    public bool IsPublishable => {(model.IsPublishable ? "true" : "false")};");
        sb.AppendLine();

        // ToMasterContent
        sb.AppendLine($"    private static {C} ToMasterContent({V} v) => new {C}");
        sb.AppendLine("    {");
        sb.AppendLine("        Id = v.Root.Id,");
        sb.AppendLine("        Name = v.Name,");
        sb.AppendLine("        Language = v.Root.MasterLanguage,");
        sb.AppendLine("        MasterLanguage = v.Root.MasterLanguage,");
        sb.AppendLine("        VersionNumber = v.VersionNumber,");
        sb.AppendLine("        Created = v.Created,");
        sb.AppendLine("        StartPublish = v.StartPublish,");
        sb.AppendLine("        StopPublish = v.StopPublish,");
        sb.AppendLine("        CreatedBy = v.CreatedBy,");
        sb.AppendLine("        PublishedBy = v.PublishedBy,");
        foreach (var p in model.InvariantProperties.Concat(model.CultureSpecificProperties))
            sb.AppendLine($"        {p.Name} = v.{p.Name},");
        sb.AppendLine("    };");
        sb.AppendLine();

        // ToTranslationContent - invariant values are borrowed live from masterSource, never stored on t.
        sb.AppendLine($"    private static {C} ToTranslationContent({Tr} t, {V} masterSource) => new {C}");
        sb.AppendLine("    {");
        sb.AppendLine("        Id = t.Root.Id,");
        sb.AppendLine("        Name = t.Name,");
        sb.AppendLine("        Language = t.Language,");
        sb.AppendLine("        MasterLanguage = t.Root.MasterLanguage,");
        sb.AppendLine("        VersionNumber = t.VersionNumber,");
        sb.AppendLine("        Created = t.Created,");
        sb.AppendLine("        StartPublish = t.StartPublish,");
        sb.AppendLine("        StopPublish = t.StopPublish,");
        sb.AppendLine("        CreatedBy = t.CreatedBy,");
        sb.AppendLine("        PublishedBy = t.PublishedBy,");
        foreach (var p in model.InvariantProperties)
            sb.AppendLine($"        {p.Name} = masterSource.{p.Name},");
        foreach (var p in model.CultureSpecificProperties)
            sb.AppendLine($"        {p.Name} = t.{p.Name},");
        sb.AppendLine("    };");
        sb.AppendLine();

        // ResolveMasterSource - published-or-latest, from an already-loaded set of a root's master versions.
        sb.AppendLine($"    private static {V} ResolveMasterSource(global::System.Collections.Generic.List<{V}> masterVersions)");
        sb.AppendLine("    {");
        sb.AppendLine("        var now = global::System.DateTime.UtcNow;");
        sb.AppendLine("        var published = masterVersions");
        sb.AppendLine($"            .Where(v => {masterWindow})");
        sb.AppendLine("            .OrderByDescending(v => v.StartPublish)");
        sb.AppendLine("            .FirstOrDefault();");
        sb.AppendLine("        return published ?? masterVersions.OrderByDescending(v => v.VersionNumber).First();");
        sb.AppendLine("    }");
        sb.AppendLine();

        if (model.UsesEffectiveVersion)
        {
            // ResolveTranslationSource - the same published-or-latest rule, for one non-master branch.
            sb.AppendLine($"    private static {Tr} ResolveTranslationSource(global::System.Collections.Generic.List<{Tr}> branchVersions)");
            sb.AppendLine("    {");
            sb.AppendLine("        var now = global::System.DateTime.UtcNow;");
            sb.AppendLine("        var published = branchVersions");
            sb.AppendLine($"            .Where(t => {translationWindow})");
            sb.AppendLine("            .OrderByDescending(t => t.StartPublish)");
            sb.AppendLine("            .FirstOrDefault();");
            sb.AppendLine("        return published ?? branchVersions.OrderByDescending(t => t.VersionNumber).First();");
            sb.AppendLine("    }");
            sb.AppendLine();

            // EffectiveVersionNumber - the version a branch resolves to while versioning or publishing is off.
            sb.AppendLine($"    private static int? EffectiveVersionNumber(CmsDbContext<{contentTypeEnum}> db, int rootId, string language, string? masterLanguage)");
            sb.AppendLine("    {");
            sb.AppendLine("        if (language == masterLanguage)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var masterVersions = db.Set<{V}>().Where(v => v.RootId == rootId).ToList();");
            sb.AppendLine("            return masterVersions.Count == 0 ? null : ResolveMasterSource(masterVersions).VersionNumber;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        var branchVersions = db.Set<{Tr}>().Where(t => t.RootId == rootId && t.Language == language).ToList();");
            sb.AppendLine("        return branchVersions.Count == 0 ? null : ResolveTranslationSource(branchVersions).VersionNumber;");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        // Create
        sb.AppendLine($"    public {C} Create(CmsDbContext<{contentTypeEnum}> db, {C} content, string language, string? userId)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var root = new global::Cms.Framework.Infrastructure.ContentRoot<{contentTypeEnum}>");
        sb.AppendLine("        {");
        sb.AppendLine("            ContentTypeKey = ContentTypeKey,");
        sb.AppendLine("            Created = global::System.DateTime.UtcNow,");
        sb.AppendLine("            MasterLanguage = language,");
        sb.AppendLine("        };");
        sb.AppendLine("        db.ContentRoots.Add(root);");
        sb.AppendLine();
        sb.AppendLine($"        var version = new {V}");
        sb.AppendLine("        {");
        sb.AppendLine("            Root = root,");
        sb.AppendLine("            Name = content.Name,");
        sb.AppendLine("            VersionNumber = 1,");
        sb.AppendLine("            Created = root.Created,");
        sb.AppendLine("            CreatedBy = userId,");
        foreach (var p in model.InvariantProperties.Concat(model.CultureSpecificProperties))
            sb.AppendLine($"            {p.Name} = content.{p.Name},");
        sb.AppendLine("        };");
        sb.AppendLine($"        db.Set<{V}>().Add(version);");
        sb.AppendLine();
        sb.AppendLine("        db.SaveChanges();");
        sb.AppendLine("        return ToMasterContent(version);");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Update
        sb.AppendLine($"    public {C} Update(CmsDbContext<{contentTypeEnum}> db, {C} content, string? userId)");
        sb.AppendLine("    {");
        sb.AppendLine("        // Each language is its own branch: numbering, history and publish state are per language.");
        sb.AppendLine("        var root = db.ContentRoots.Single(r => r.Id == content.Id);");
        sb.AppendLine();
        if (model.IsVersioned) EmitAppendVersionUpdateBody(sb, model);
        else EmitInPlaceUpdateBody(sb, model);
        sb.AppendLine("    }");
        sb.AppendLine();

        // QueryCurrent
        sb.AppendLine($"    public global::System.Linq.IQueryable<{C}> QueryCurrent(CmsDbContext<{contentTypeEnum}> db, string? language, bool publishedOnly = false)");
        sb.AppendLine("    {");
        if (!model.IsPublishable)
        {
            sb.AppendLine("        // Not publishable: each branch's effective version is always its live one, so both modes read the same rows.");
            sb.AppendLine("        return language is null");
            sb.AppendLine($"            ? db.Set<{C}>().Where(x => x.Language == x.MasterLanguage)");
            sb.AppendLine($"            : db.Set<{C}>().Where(x => x.Language == language);");
            sb.AppendLine("    }");
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("        if (!publishedOnly)");
            sb.AppendLine("        {");
            sb.AppendLine("            return language is null");
            sb.AppendLine($"                ? db.Set<{C}>().Where(x => x.Language == x.MasterLanguage)");
            sb.AppendLine($"                : db.Set<{C}>().Where(x => x.Language == language);");
            sb.AppendLine("        }");
            sb.AppendLine();
            EmitLiveQueryBody(sb, model);
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        // QueryHistory
        sb.AppendLine($"    public global::System.Collections.Generic.IReadOnlyList<{C}> QueryHistory(CmsDbContext<{contentTypeEnum}> db, int id, string? language)");
        sb.AppendLine("    {");
        sb.AppendLine("        var root = db.ContentRoots.FirstOrDefault(r => r.Id == id);");
        sb.AppendLine($"        if (root is null) return global::System.Array.Empty<{C}>();");
        sb.AppendLine();
        sb.AppendLine("        var branch = language ?? root.MasterLanguage;");
        sb.AppendLine("        if (branch == root.MasterLanguage)");
        sb.AppendLine("        {");
        sb.AppendLine($"            return db.Set<{V}>()");
        sb.AppendLine("                .Where(v => v.RootId == id)");
        sb.AppendLine("                .OrderByDescending(v => v.VersionNumber)");
        sb.AppendLine("                .Include(v => v.Root)");
        sb.AppendLine("                .ToList()");
        sb.AppendLine("                .Select(ToMasterContent)");
        sb.AppendLine("                .ToList();");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        var translations = db.Set<{Tr}>()");
        sb.AppendLine("            .Where(t => t.RootId == id && t.Language == branch)");
        sb.AppendLine("            .OrderByDescending(t => t.VersionNumber)");
        sb.AppendLine("            .Include(t => t.Root)");
        sb.AppendLine("            .ToList();");
        sb.AppendLine($"        if (translations.Count == 0) return global::System.Array.Empty<{C}>();");
        sb.AppendLine();
        sb.AppendLine("        // The same, currently-resolved master source for every row: history is not a");
        sb.AppendLine("        // snapshot of what the invariant values were at each version, only of the culture text.");
        sb.AppendLine($"        var masterSource = ResolveMasterSource(db.Set<{V}>().Where(v => v.RootId == id).ToList());");
        sb.AppendLine("        return translations.Select(t => ToTranslationContent(t, masterSource)).ToList();");
        sb.AppendLine("    }");
        sb.AppendLine();

        // QueryLanguages
        sb.AppendLine($"    public global::System.Collections.Generic.IReadOnlyDictionary<int, global::System.Collections.Generic.IReadOnlyList<string>> QueryLanguages(CmsDbContext<{contentTypeEnum}> db, global::System.Collections.Generic.IReadOnlyCollection<int> ids)");
        sb.AppendLine("    {");
        sb.AppendLine("        var master = db.ContentRoots.Where(r => ids.Contains(r.Id)).Select(r => new { RootId = r.Id, Language = r.MasterLanguage }).ToList();");
        sb.AppendLine($"        var translated = db.Set<{Tr}>().Where(t => ids.Contains(t.RootId)).Select(t => new {{ t.RootId, t.Language }}).Distinct().ToList();");
        sb.AppendLine("        return master");
        sb.AppendLine("            .Concat(translated)");
        sb.AppendLine("            .GroupBy(r => r.RootId)");
        sb.AppendLine("            .ToDictionary(g => g.Key, g => (global::System.Collections.Generic.IReadOnlyList<string>)g.Select(r => r.Language).Distinct().OrderBy(l => l, global::System.StringComparer.Ordinal).ToList());");
        sb.AppendLine("    }");
        sb.AppendLine();

        // QueryLanguageStatuses
        sb.AppendLine($"    public global::System.Collections.Generic.IReadOnlyDictionary<int, global::System.Collections.Generic.IReadOnlyList<global::Cms.Framework.Infrastructure.LanguageStatusDto>> QueryLanguageStatuses(CmsDbContext<{contentTypeEnum}> db, global::System.Collections.Generic.IReadOnlyCollection<int> ids)");
        sb.AppendLine("    {");
        sb.AppendLine("        var now = global::System.DateTime.UtcNow;");
        sb.AppendLine("        var masterLanguageByRoot = db.ContentRoots.Where(r => ids.Contains(r.Id)).ToDictionary(r => r.Id, r => r.MasterLanguage);");
        sb.AppendLine();
        sb.AppendLine($"        var masterRows = db.Set<{V}>().Where(v => ids.Contains(v.RootId)).Select(v => new {{ v.RootId, v.VersionNumber, v.StartPublish, v.StopPublish }}).ToList();");
        sb.AppendLine("        var masterEntries = masterRows");
        sb.AppendLine("            .GroupBy(v => v.RootId)");
        sb.AppendLine("            .Select(g =>");
        sb.AppendLine("            {");
        sb.AppendLine($"                var latest = {BranchCurrentExpression("v", masterWindow)};");
        sb.AppendLine(model.IsPublishable ? $"                var live = g.Where(v => {masterWindow}).OrderByDescending(v => v.StartPublish).Select(v => (int?)v.VersionNumber).FirstOrDefault();" : "                var live = (int?)latest.VersionNumber;");
        sb.AppendLine("                return (RootId: g.Key, Status: new global::Cms.Framework.Infrastructure.LanguageStatusDto");
        sb.AppendLine("                {");
        sb.AppendLine("                    Language = masterLanguageByRoot[g.Key],");
        sb.AppendLine("                    VersionNumber = latest.VersionNumber,");
        sb.AppendLine("                    StartPublish = latest.StartPublish,");
        sb.AppendLine("                    LivePublishedVersionNumber = live,");
        sb.AppendLine(model.IsPublishable ? "                    HasBeenPublished = g.Any(v => v.StartPublish != null)," : "                    HasBeenPublished = true,");
        sb.AppendLine("                });");
        sb.AppendLine("            })");
        sb.AppendLine("            .ToList();");
        sb.AppendLine();
        sb.AppendLine($"        var translationRows = db.Set<{Tr}>().Where(t => ids.Contains(t.RootId)).Select(t => new {{ t.RootId, t.Language, t.VersionNumber, t.StartPublish, t.StopPublish }}).ToList();");
        sb.AppendLine("        var translationEntries = translationRows");
        sb.AppendLine("            .GroupBy(t => new { t.RootId, t.Language })");
        sb.AppendLine("            .Select(g =>");
        sb.AppendLine("            {");
        sb.AppendLine($"                var latest = {BranchCurrentExpression("t", translationWindow)};");
        sb.AppendLine(model.IsPublishable ? $"                var live = g.Where(t => {translationWindow}).OrderByDescending(t => t.StartPublish).Select(t => (int?)t.VersionNumber).FirstOrDefault();" : "                var live = (int?)latest.VersionNumber;");
        sb.AppendLine("                return (RootId: g.Key.RootId, Status: new global::Cms.Framework.Infrastructure.LanguageStatusDto");
        sb.AppendLine("                {");
        sb.AppendLine("                    Language = g.Key.Language,");
        sb.AppendLine("                    VersionNumber = latest.VersionNumber,");
        sb.AppendLine("                    StartPublish = latest.StartPublish,");
        sb.AppendLine("                    LivePublishedVersionNumber = live,");
        sb.AppendLine(model.IsPublishable ? "                    HasBeenPublished = g.Any(t => t.StartPublish != null)," : "                    HasBeenPublished = true,");
        sb.AppendLine("                });");
        sb.AppendLine("            });");
        sb.AppendLine();
        sb.AppendLine("        return masterEntries");
        sb.AppendLine("            .Concat(translationEntries)");
        sb.AppendLine("            .GroupBy(e => e.RootId)");
        sb.AppendLine("            .ToDictionary(g => g.Key, g => (global::System.Collections.Generic.IReadOnlyList<global::Cms.Framework.Infrastructure.LanguageStatusDto>)g.Select(e => e.Status).ToList());");
        sb.AppendLine("    }");
        sb.AppendLine();

        // QueryLastModified
        sb.AppendLine($"    public global::System.Collections.Generic.IReadOnlyDictionary<int, global::System.DateTime> QueryLastModified(CmsDbContext<{contentTypeEnum}> db, global::System.Collections.Generic.IReadOnlyCollection<int> ids)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var masterMax = db.Set<{V}>().Where(v => ids.Contains(v.RootId)).GroupBy(v => v.RootId).Select(g => new {{ RootId = g.Key, Created = g.Max(v => v.Created) }});");
        sb.AppendLine($"        var translationMax = db.Set<{Tr}>().Where(t => ids.Contains(t.RootId)).GroupBy(t => t.RootId).Select(g => new {{ RootId = g.Key, Created = g.Max(t => t.Created) }});");
        sb.AppendLine("        return masterMax");
        sb.AppendLine("            .Concat(translationMax)");
        sb.AppendLine("            .ToList()");
        sb.AppendLine("            .GroupBy(x => x.RootId)");
        sb.AppendLine("            .ToDictionary(g => g.Key, g => g.Max(x => x.Created));");
        sb.AppendLine("    }");
        sb.AppendLine();

        // QueryFirstPublished - reliable only because a start date, once reached, is never changed (see SetPublishSchedule).
        sb.AppendLine($"    public global::System.Collections.Generic.IReadOnlyDictionary<(int RootId, string Language), global::System.DateTime> QueryFirstPublished(CmsDbContext<{contentTypeEnum}> db, global::System.Collections.Generic.IReadOnlyCollection<int> ids)");
        sb.AppendLine("    {");
        sb.AppendLine("        var now = global::System.DateTime.UtcNow;");
        sb.AppendLine($"        var master = db.Set<{V}>()");
        sb.AppendLine("            .Where(v => ids.Contains(v.RootId) && v.StartPublish != null && v.StartPublish <= now)");
        sb.AppendLine("            .GroupBy(v => new { v.RootId, v.Root.MasterLanguage })");
        sb.AppendLine("            .Select(g => new { g.Key.RootId, Language = g.Key.MasterLanguage, FirstPublished = g.Min(v => v.StartPublish) })");
        sb.AppendLine("            .ToList();");
        sb.AppendLine($"        var translations = db.Set<{Tr}>()");
        sb.AppendLine("            .Where(t => ids.Contains(t.RootId) && t.StartPublish != null && t.StartPublish <= now)");
        sb.AppendLine("            .GroupBy(t => new { t.RootId, t.Language })");
        sb.AppendLine("            .Select(g => new { g.Key.RootId, g.Key.Language, FirstPublished = g.Min(t => t.StartPublish) })");
        sb.AppendLine("            .ToList();");
        sb.AppendLine("        return master");
        sb.AppendLine("            .Concat(translations)");
        sb.AppendLine("            .ToDictionary(x => (x.RootId, x.Language), x => x.FirstPublished!.Value);");
        sb.AppendLine("    }");
        sb.AppendLine();

        // GetVersionNumber
        sb.AppendLine($"    public int? GetVersionNumber(CmsDbContext<{contentTypeEnum}> db, int rootId, string language, bool publishedOnly)");
        sb.AppendLine("    {");
        sb.AppendLine("        var masterLanguage = db.ContentRoots.Where(r => r.Id == rootId).Select(r => r.MasterLanguage).FirstOrDefault();");
        if (!model.IsPublishable)
        {
            sb.AppendLine();
            sb.AppendLine("        // Not publishable: the effective version is both the current and the live one.");
            sb.AppendLine("        return EffectiveVersionNumber(db, rootId, language, masterLanguage);");
            sb.AppendLine("    }");
            sb.AppendLine();
        }
        else
        {
            EmitPublishableGetVersionNumberBody(sb, model);
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        // VersionExists
        sb.AppendLine($"    public bool VersionExists(CmsDbContext<{contentTypeEnum}> db, int rootId, string language, int versionNumber)");
        sb.AppendLine("    {");
        sb.AppendLine("        var masterLanguage = db.ContentRoots.Where(r => r.Id == rootId).Select(r => r.MasterLanguage).FirstOrDefault();");
        sb.AppendLine("        return language == masterLanguage");
        sb.AppendLine($"            ? db.Set<{V}>().Any(v => v.RootId == rootId && v.VersionNumber == versionNumber)");
        sb.AppendLine($"            : db.Set<{Tr}>().Any(t => t.RootId == rootId && t.Language == language && t.VersionNumber == versionNumber);");
        sb.AppendLine("    }");
        sb.AppendLine();

        // SetPublishSchedule
        sb.AppendLine($"    public void SetPublishSchedule(CmsDbContext<{contentTypeEnum}> db, int rootId, string language, int versionNumber, global::System.DateTime? startPublish, global::System.DateTime? stopPublish, string? userId)");
        sb.AppendLine("    {");
        sb.AppendLine("        var masterLanguage = db.ContentRoots.Where(r => r.Id == rootId).Select(r => r.MasterLanguage).FirstOrDefault();");
        sb.AppendLine("        var now = global::System.DateTime.UtcNow;");
        sb.AppendLine("        if (language == masterLanguage)");
        sb.AppendLine("        {");
        sb.AppendLine($"            var version = db.Set<{V}>().Single(v => v.RootId == rootId && v.VersionNumber == versionNumber);");
        sb.AppendLine("            EnsureStartUnchangedOnceReached(version.StartPublish, startPublish, now);");
        sb.AppendLine("            version.StartPublish = startPublish;");
        sb.AppendLine("            version.StopPublish = stopPublish;");
        sb.AppendLine("            version.PublishedBy = userId;");
        sb.AppendLine("        }");
        sb.AppendLine("        else");
        sb.AppendLine("        {");
        sb.AppendLine($"            var translation = db.Set<{Tr}>().Single(t => t.RootId == rootId && t.Language == language && t.VersionNumber == versionNumber);");
        sb.AppendLine("            EnsureStartUnchangedOnceReached(translation.StartPublish, startPublish, now);");
        sb.AppendLine("            translation.StartPublish = startPublish;");
        sb.AppendLine("            translation.StopPublish = stopPublish;");
        sb.AppendLine("            translation.PublishedBy = userId;");
        sb.AppendLine("        }");
        sb.AppendLine("        db.SaveChanges();");
        sb.AppendLine("    }");
        sb.AppendLine();

        // A start date that has been reached is the record of when that version went live; first-published
        // dates are derived from it, so it must never move. Republishing such a version goes through CopyVersion.
        sb.AppendLine("    private static void EnsureStartUnchangedOnceReached(global::System.DateTime? current, global::System.DateTime? requested, global::System.DateTime now)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (current is { } start && start <= now && requested != current)");
        sb.AppendLine("            throw new global::System.InvalidOperationException(\"A version's start publish date cannot be changed once it has been reached.\");");
        sb.AppendLine("    }");
        sb.AppendLine();

        // CopyVersion
        sb.AppendLine($"    public int CopyVersion(CmsDbContext<{contentTypeEnum}> db, int rootId, string language, int versionNumber, string? userId)");
        sb.AppendLine("    {");
        sb.AppendLine("        var root = db.ContentRoots.Single(r => r.Id == rootId);");
        sb.AppendLine("        if (language == root.MasterLanguage)");
        sb.AppendLine("        {");
        sb.AppendLine($"            var source = db.Set<{V}>().Single(v => v.RootId == rootId && v.VersionNumber == versionNumber);");
        sb.AppendLine($"            var nextVersionNumber = db.Set<{V}>().Where(v => v.RootId == rootId).Max(v => v.VersionNumber) + 1;");
        sb.AppendLine($"            db.Set<{V}>().Add(new {V}");
        sb.AppendLine("            {");
        sb.AppendLine("                Root = root,");
        sb.AppendLine("                Name = source.Name,");
        sb.AppendLine("                VersionNumber = nextVersionNumber,");
        sb.AppendLine("                Created = global::System.DateTime.UtcNow,");
        sb.AppendLine("                CreatedBy = userId,");
        foreach (var p in model.InvariantProperties.Concat(model.CultureSpecificProperties))
            sb.AppendLine($"                {p.Name} = source.{p.Name},");
        sb.AppendLine("            });");
        sb.AppendLine("            db.SaveChanges();");
        sb.AppendLine("            return nextVersionNumber;");
        sb.AppendLine("        }");
        sb.AppendLine("        else");
        sb.AppendLine("        {");
        sb.AppendLine($"            var source = db.Set<{Tr}>().Single(t => t.RootId == rootId && t.Language == language && t.VersionNumber == versionNumber);");
        sb.AppendLine($"            var nextVersionNumber = db.Set<{Tr}>().Where(t => t.RootId == rootId && t.Language == language).Max(t => t.VersionNumber) + 1;");
        sb.AppendLine($"            db.Set<{Tr}>().Add(new {Tr}");
        sb.AppendLine("            {");
        sb.AppendLine("                Root = root,");
        sb.AppendLine("                Name = source.Name,");
        sb.AppendLine("                Language = language,");
        sb.AppendLine("                VersionNumber = nextVersionNumber,");
        sb.AppendLine("                Created = global::System.DateTime.UtcNow,");
        sb.AppendLine("                CreatedBy = userId,");
        foreach (var p in model.CultureSpecificProperties)
            sb.AppendLine($"                {p.Name} = source.{p.Name},");
        sb.AppendLine("            });");
        sb.AppendLine("            db.SaveChanges();");
        sb.AppendLine("            return nextVersionNumber;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        // StopActivePublish
        sb.AppendLine($"    public bool StopActivePublish(CmsDbContext<{contentTypeEnum}> db, int rootId, string language, global::System.DateTime stopAt, string? userId)");
        sb.AppendLine("    {");
        sb.AppendLine("        var masterLanguage = db.ContentRoots.Where(r => r.Id == rootId).Select(r => r.MasterLanguage).FirstOrDefault();");
        sb.AppendLine("        var now = global::System.DateTime.UtcNow;");
        sb.AppendLine();
        sb.AppendLine("        if (language == masterLanguage)");
        sb.AppendLine("        {");
        sb.AppendLine($"            var active = db.Set<{V}>()");
        sb.AppendLine($"                .Where(v => v.RootId == rootId && {masterWindow})");
        sb.AppendLine("                .OrderByDescending(v => v.StartPublish)");
        sb.AppendLine("                .FirstOrDefault();");
        sb.AppendLine("            if (active is null) return false;");
        sb.AppendLine("            active.StopPublish = stopAt;");
        sb.AppendLine("            active.PublishedBy = userId;");
        sb.AppendLine("        }");
        sb.AppendLine("        else");
        sb.AppendLine("        {");
        sb.AppendLine($"            var active = db.Set<{Tr}>()");
        sb.AppendLine($"                .Where(t => t.RootId == rootId && t.Language == language && {translationWindow})");
        sb.AppendLine("                .OrderByDescending(t => t.StartPublish)");
        sb.AppendLine("                .FirstOrDefault();");
        sb.AppendLine("            if (active is null) return false;");
        sb.AppendLine("            active.StopPublish = stopAt;");
        sb.AppendLine("            active.PublishedBy = userId;");
        sb.AppendLine("        }");
        sb.AppendLine("        db.SaveChanges();");
        sb.AppendLine("        return true;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // RemoveUserReferences
        sb.AppendLine($"    public void RemoveUserReferences(CmsDbContext<{contentTypeEnum}> db, string userId)");
        sb.AppendLine("    {");
        sb.AppendLine($"        db.Set<{V}>().Where(v => v.CreatedBy == userId).ExecuteUpdate(s => s.SetProperty(v => v.CreatedBy, (string?)null));");
        sb.AppendLine($"        db.Set<{V}>().Where(v => v.PublishedBy == userId).ExecuteUpdate(s => s.SetProperty(v => v.PublishedBy, (string?)null));");
        sb.AppendLine($"        db.Set<{Tr}>().Where(t => t.CreatedBy == userId).ExecuteUpdate(s => s.SetProperty(t => t.CreatedBy, (string?)null));");
        sb.AppendLine($"        db.Set<{Tr}>().Where(t => t.PublishedBy == userId).ExecuteUpdate(s => s.SetProperty(t => t.PublishedBy, (string?)null));");
        sb.AppendLine();
        sb.AppendLine("        // ExecuteUpdate bypasses the change tracker; bring rows this context already");
        sb.AppendLine("        // loaded in line with the database so later reads don't serve the removed id.");
        sb.AppendLine($"        foreach (var entry in db.ChangeTracker.Entries<{V}>())");
        sb.AppendLine("        {");
        sb.AppendLine("            if (entry.Entity.CreatedBy == userId)");
        sb.AppendLine("            {");
        sb.AppendLine("                entry.Property(v => v.CreatedBy).CurrentValue = null;");
        sb.AppendLine("                entry.Property(v => v.CreatedBy).OriginalValue = null;");
        sb.AppendLine("            }");
        sb.AppendLine("            if (entry.Entity.PublishedBy == userId)");
        sb.AppendLine("            {");
        sb.AppendLine("                entry.Property(v => v.PublishedBy).CurrentValue = null;");
        sb.AppendLine("                entry.Property(v => v.PublishedBy).OriginalValue = null;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine($"        foreach (var entry in db.ChangeTracker.Entries<{Tr}>())");
        sb.AppendLine("        {");
        sb.AppendLine("            if (entry.Entity.CreatedBy == userId)");
        sb.AppendLine("            {");
        sb.AppendLine("                entry.Property(t => t.CreatedBy).CurrentValue = null;");
        sb.AppendLine("                entry.Property(t => t.CreatedBy).OriginalValue = null;");
        sb.AppendLine("            }");
        sb.AppendLine("            if (entry.Entity.PublishedBy == userId)");
        sb.AppendLine("            {");
        sb.AppendLine("                entry.Property(t => t.PublishedBy).CurrentValue = null;");
        sb.AppendLine("                entry.Property(t => t.PublishedBy).OriginalValue = null;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("    }");

        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// The <c>publishedOnly</c> half of <c>QueryCurrent</c> for a publishable type:
    /// each branch's version whose publish window contains now, omitting branches with none.
    /// </summary>
    private static void EmitLiveQueryBody(StringBuilder sb, ContentTypeModel model)
    {
        var V = model.VersionTypeName;
        var Tr = model.TranslationTypeName;
        const string masterWindow = MasterWindow;
        const string translationWindow = TranslationWindow;

        sb.AppendLine("        var now = global::System.DateTime.UtcNow;");
        sb.AppendLine();
        sb.AppendLine("        if (language is null)");
        sb.AppendLine("        {");
        sb.AppendLine("            // Each item's own master-language branch, only when that branch is itself live.");
        sb.AppendLine($"            return db.Set<{V}>()");
        sb.AppendLine($"                .Where(v => {masterWindow})");
        sb.AppendLine("                .Include(v => v.Root)");
        sb.AppendLine("                .ToList()");
        sb.AppendLine("                .GroupBy(v => v.RootId)");
        sb.AppendLine("                .Select(g => ToMasterContent(g.OrderByDescending(v => v.StartPublish).First()))");
        sb.AppendLine("                .AsQueryable();");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        // Roots whose master language is the one requested: read their own live master row.");
        sb.AppendLine($"        var liveMaster = db.Set<{V}>()");
        sb.AppendLine($"            .Where(v => {masterWindow})");
        sb.AppendLine("            .Include(v => v.Root)");
        sb.AppendLine("            .ToList()");
        sb.AppendLine("            .Where(v => v.Root.MasterLanguage == language)");
        sb.AppendLine("            .GroupBy(v => v.RootId)");
        sb.AppendLine("            .Select(g => ToMasterContent(g.OrderByDescending(v => v.StartPublish).First()));");
        sb.AppendLine();
        sb.AppendLine("        // Every other root: read the live translation for this language, own window, and");
        sb.AppendLine("        // resolve its invariant values live from that root's master branch.");
        sb.AppendLine($"        var liveTranslations = db.Set<{Tr}>()");
        sb.AppendLine($"            .Where(t => t.Language == language && {translationWindow})");
        sb.AppendLine("            .Include(t => t.Root)");
        sb.AppendLine("            .ToList()");
        sb.AppendLine("            .GroupBy(t => t.RootId)");
        sb.AppendLine("            .Select(g => g.OrderByDescending(t => t.StartPublish).First())");
        sb.AppendLine("            .ToList();");
        sb.AppendLine();
        sb.AppendLine("        var translatedRootIds = liveTranslations.Select(t => t.RootId).ToList();");
        sb.AppendLine($"        var masterVersionsByRoot = db.Set<{V}>()");
        sb.AppendLine("            .Where(v => translatedRootIds.Contains(v.RootId))");
        sb.AppendLine("            .ToList()");
        sb.AppendLine("            .GroupBy(v => v.RootId)");
        sb.AppendLine("            .ToDictionary(g => g.Key, g => g.ToList());");
        sb.AppendLine();
        sb.AppendLine("        var resolvedTranslations = liveTranslations.Select(t => ToTranslationContent(t, ResolveMasterSource(masterVersionsByRoot[t.RootId])));");
        sb.AppendLine();
        sb.AppendLine("        return liveMaster.Concat(resolvedTranslations).AsQueryable();");
    }

    /// <summary>
    /// <c>GetVersionNumber</c> for a publishable type: the live version by publish window,
    /// and as the current one the latest - or, when not versioned, the effective version.
    /// </summary>
    private static void EmitPublishableGetVersionNumberBody(StringBuilder sb, ContentTypeModel model)
    {
        var V = model.VersionTypeName;
        var Tr = model.TranslationTypeName;
        const string masterWindow = MasterWindow;
        const string translationWindow = TranslationWindow;

        sb.AppendLine("        var now = global::System.DateTime.UtcNow;");
        sb.AppendLine();
        sb.AppendLine("        if (!publishedOnly)");
        sb.AppendLine("        {");
        if (model.UsesEffectiveVersion)
        {
            sb.AppendLine("            return EffectiveVersionNumber(db, rootId, language, masterLanguage);");
        }
        else
        {
            sb.AppendLine("            if (language == masterLanguage)");
            sb.AppendLine($"                return db.Set<{V}>()");
            sb.AppendLine("                    .Where(v => v.RootId == rootId)");
            sb.AppendLine("                    .OrderByDescending(v => v.VersionNumber)");
            sb.AppendLine("                    .Select(v => (int?)v.VersionNumber)");
            sb.AppendLine("                    .FirstOrDefault();");
            sb.AppendLine();
            sb.AppendLine($"            return db.Set<{Tr}>()");
            sb.AppendLine("                .Where(t => t.RootId == rootId && t.Language == language)");
            sb.AppendLine("                .OrderByDescending(t => t.VersionNumber)");
            sb.AppendLine("                .Select(t => (int?)t.VersionNumber)");
            sb.AppendLine("                .FirstOrDefault();");
        }
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        if (language == masterLanguage)");
        sb.AppendLine($"            return db.Set<{V}>()");
        sb.AppendLine($"                .Where(v => v.RootId == rootId && {masterWindow})");
        sb.AppendLine("                .OrderByDescending(v => v.StartPublish)");
        sb.AppendLine("                .Select(v => (int?)v.VersionNumber)");
        sb.AppendLine("                .FirstOrDefault();");
        sb.AppendLine();
        sb.AppendLine($"        return db.Set<{Tr}>()");
        sb.AppendLine($"            .Where(t => t.RootId == rootId && t.Language == language && {translationWindow})");
        sb.AppendLine("            .OrderByDescending(t => t.StartPublish)");
        sb.AppendLine("            .Select(t => (int?)t.VersionNumber)");
        sb.AppendLine("            .FirstOrDefault();");
    }

    /// <summary><c>Update</c> for a versioned type: adds a new version to the saved branch.</summary>
    private static void EmitAppendVersionUpdateBody(StringBuilder sb, ContentTypeModel model)
    {
        var V = model.VersionTypeName;
        var Tr = model.TranslationTypeName;

        sb.AppendLine("        if (content.Language == root.MasterLanguage)");
        sb.AppendLine("        {");
        sb.AppendLine($"            var nextVersionNumber = (db.Set<{V}>().Where(v => v.RootId == content.Id).Select(v => (int?)v.VersionNumber).Max() ?? 0) + 1;");
        sb.AppendLine($"            var version = new {V}");
        sb.AppendLine("            {");
        sb.AppendLine("                Root = root,");
        sb.AppendLine("                Name = content.Name,");
        sb.AppendLine("                VersionNumber = nextVersionNumber,");
        sb.AppendLine("                Created = global::System.DateTime.UtcNow,");
        sb.AppendLine("                CreatedBy = userId,");
        foreach (var p in model.InvariantProperties.Concat(model.CultureSpecificProperties))
            sb.AppendLine($"                {p.Name} = content.{p.Name},");
        sb.AppendLine("            };");
        sb.AppendLine($"            db.Set<{V}>().Add(version);");
        sb.AppendLine("            db.SaveChanges();");
        sb.AppendLine("            return ToMasterContent(version);");
        sb.AppendLine("        }");
        sb.AppendLine("        else");
        sb.AppendLine("        {");
        sb.AppendLine($"            var nextVersionNumber = (db.Set<{Tr}>().Where(t => t.RootId == content.Id && t.Language == content.Language).Select(t => (int?)t.VersionNumber).Max() ?? 0) + 1;");
        sb.AppendLine($"            var translation = new {Tr}");
        sb.AppendLine("            {");
        sb.AppendLine("                Root = root,");
        sb.AppendLine("                Name = content.Name,");
        sb.AppendLine("                Language = content.Language,");
        sb.AppendLine("                VersionNumber = nextVersionNumber,");
        sb.AppendLine("                Created = global::System.DateTime.UtcNow,");
        sb.AppendLine("                CreatedBy = userId,");
        foreach (var p in model.CultureSpecificProperties)
            sb.AppendLine($"                {p.Name} = content.{p.Name},");
        sb.AppendLine("            };");
        sb.AppendLine($"            db.Set<{Tr}>().Add(translation);");
        sb.AppendLine("            db.SaveChanges();");
        sb.AppendLine();
        sb.AppendLine($"            var masterVersions = db.Set<{V}>().Where(v => v.RootId == content.Id).ToList();");
        sb.AppendLine("            return ToTranslationContent(translation, ResolveMasterSource(masterVersions));");
        sb.AppendLine("        }");
    }

    /// <summary>
    /// <c>Update</c> for a type that isn't versioned: overwrites each branch's
    /// effective version (published, else latest) instead of adding a new one.
    /// Its version number and publish window are left as they are, so a live
    /// row stays live with the new values; any newer, unpublished rows left
    /// from when the type was versioned are kept, untouched, as history.
    /// </summary>
    private static void EmitInPlaceUpdateBody(StringBuilder sb, ContentTypeModel model)
    {
        var V = model.VersionTypeName;
        var Tr = model.TranslationTypeName;

        sb.AppendLine("        // Not versioned: overwrite the branch's effective version in place.");
        sb.AppendLine("        if (content.Language == root.MasterLanguage)");
        sb.AppendLine("        {");
        sb.AppendLine($"            var version = ResolveMasterSource(db.Set<{V}>().Where(v => v.RootId == content.Id).ToList());");
        sb.AppendLine("            version.Name = content.Name;");
        sb.AppendLine("            version.Created = global::System.DateTime.UtcNow;");
        sb.AppendLine("            version.CreatedBy = userId;");
        foreach (var p in model.InvariantProperties.Concat(model.CultureSpecificProperties))
            sb.AppendLine($"            version.{p.Name} = content.{p.Name};");
        sb.AppendLine("            db.SaveChanges();");
        sb.AppendLine("            return ToMasterContent(version);");
        sb.AppendLine("        }");
        sb.AppendLine("        else");
        sb.AppendLine("        {");
        sb.AppendLine($"            var branchVersions = db.Set<{Tr}>().Where(t => t.RootId == content.Id && t.Language == content.Language).ToList();");
        sb.AppendLine("            var translation = branchVersions.Count == 0 ? null : ResolveTranslationSource(branchVersions);");
        sb.AppendLine("            if (translation is null)");
        sb.AppendLine("            {");
        sb.AppendLine($"                translation = new {Tr} {{ Root = root, Language = content.Language, VersionNumber = 1 }};");
        sb.AppendLine($"                db.Set<{Tr}>().Add(translation);");
        sb.AppendLine("            }");
        sb.AppendLine("            translation.Name = content.Name;");
        sb.AppendLine("            translation.Created = global::System.DateTime.UtcNow;");
        sb.AppendLine("            translation.CreatedBy = userId;");
        foreach (var p in model.CultureSpecificProperties)
            sb.AppendLine($"            translation.{p.Name} = content.{p.Name};");
        sb.AppendLine("            db.SaveChanges();");
        sb.AppendLine();
        sb.AppendLine($"            var masterVersions = db.Set<{V}>().Where(v => v.RootId == content.Id).ToList();");
        sb.AppendLine("            return ToTranslationContent(translation, ResolveMasterSource(masterVersions));");
        sb.AppendLine("        }");
    }

    public static string EmitRegistration(IReadOnlyList<ContentTypeModel> models, string enumNamespace)
    {
        var contentTypeEnum = ContentTypeEnumFullName(enumNamespace);
        var sb = new StringBuilder();
        AppendHeader(sb);
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Cms.Framework.Infrastructure;");
        sb.AppendLine();
        sb.AppendLine($"[assembly: global::Cms.Framework.Infrastructure.ContentTypeRegistrar(typeof({GeneratedNamespace}.GeneratedContentTypeRegistrar))]");
        sb.AppendLine();
        sb.AppendLine($"namespace {GeneratedNamespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Registers every [ContentType] class discovered in this assembly - one");
        sb.AppendLine("/// generated IContentTypeStore&lt;T&gt; and IContentTypeMetadata per type.");
        sb.AppendLine("/// Internal, so each content assembly gets its own copy without clashing;");
        sb.AppendLine("/// AddCms finds it through the assembly-level attribute above.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("internal sealed class GeneratedContentTypeRegistrar : IContentTypeRegistrar");
        sb.AppendLine("{");
        sb.AppendLine("    public void Register(IServiceCollection services)");
        sb.AppendLine("    {");
        foreach (var model in models)
        {
            sb.AppendLine($"        services.AddSingleton<IContentTypeStore<{model.FullyQualifiedName}, {contentTypeEnum}>, {model.StoreTypeName}>();");
            sb.AppendLine($"        services.AddSingleton<IContentTypeMetadata<{contentTypeEnum}>, ContentTypeMetadata<{model.FullyQualifiedName}, {contentTypeEnum}>>();");
        }
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// A flat, per-language row for every content item: one half for the
    /// master-language branch (invariant + master's own culture text, both
    /// straight from its latest <c>{Type}Version</c>), unioned with one half
    /// per non-master language (culture text from its latest
    /// <c>{Type}Translation</c>, invariant values resolved live - published,
    /// or with nothing published, latest - from the item's master branch).
    /// This is always the "current" shape; <c>publishedOnly</c> queries are
    /// resolved separately, in code, since they need each branch's own window.
    /// While the type isn't versioned or isn't publishable, "latest" above is
    /// the branch's effective version (published, else latest) instead.
    /// </summary>
    private static string BuildReadSql(ContentTypeModel model)
    {
        var masterInvariantCols = string.Join(string.Empty, model.InvariantProperties.Select(p => $",\n    v.{p.Name} AS {p.Name}"));
        var masterCultureCols = string.Join(string.Empty, model.CultureSpecificProperties.Select(p => $",\n    v.{p.Name} AS {p.Name}"));
        var resolvedInvariantCols = string.Join(string.Empty, model.InvariantProperties.Select(p => $",\n    mv.{p.Name} AS {p.Name}"));
        var translationCultureCols = string.Join(string.Empty, model.CultureSpecificProperties.Select(p => $",\n    t.{p.Name} AS {p.Name}"));

        // The master-branch version currently in effect: published if
        // something is, otherwise the latest. Every non-master branch's
        // invariant values resolve from this row, live, never copied.
        // EF stores DateTime as TEXT with sub-second precision, which sorts
        // after a plain datetime('now'); normalize both sides through
        // datetime(...) so the comparison isn't always false.
        var masterSourceId =
$@"COALESCE(
        (SELECT v3.Id FROM {model.VersionTableName} v3
         WHERE v3.RootId = r.Id AND v3.StartPublish IS NOT NULL AND datetime(v3.StartPublish) <= datetime('now') AND (v3.StopPublish IS NULL OR datetime(v3.StopPublish) > datetime('now'))
         ORDER BY v3.StartPublish DESC LIMIT 1),
        (SELECT v4.Id FROM {model.VersionTableName} v4 WHERE v4.RootId = r.Id ORDER BY v4.VersionNumber DESC LIMIT 1)
    )";

        // Which row is each branch's current one: the latest, or - while
        // versioning or publishing is off - the effective (published, else
        // latest) version, by the same rule as the master source above.
        var masterCurrent = model.UsesEffectiveVersion
            ? $"v.Id = ({masterSourceId})"
            : $"v.VersionNumber = (SELECT MAX(v2.VersionNumber) FROM {model.VersionTableName} v2 WHERE v2.RootId = r.Id)";
        var translationCurrent = model.UsesEffectiveVersion
            ? $@"t.Id = COALESCE(
        (SELECT t3.Id FROM {model.TranslationTableName} t3
         WHERE t3.RootId = r.Id AND t3.Language = t.Language AND t3.StartPublish IS NOT NULL AND datetime(t3.StartPublish) <= datetime('now') AND (t3.StopPublish IS NULL OR datetime(t3.StopPublish) > datetime('now'))
         ORDER BY t3.StartPublish DESC LIMIT 1),
        (SELECT t4.Id FROM {model.TranslationTableName} t4 WHERE t4.RootId = r.Id AND t4.Language = t.Language ORDER BY t4.VersionNumber DESC LIMIT 1)
    )"
            : $"t.VersionNumber = (SELECT MAX(t2.VersionNumber) FROM {model.TranslationTableName} t2 WHERE t2.RootId = r.Id AND t2.Language = t.Language)";

        return
$@"SELECT
    r.Id AS Id,
    v.Name AS Name,
    r.MasterLanguage AS Language,
    r.MasterLanguage AS MasterLanguage,
    v.VersionNumber AS VersionNumber,
    v.Created AS Created,
    v.StartPublish AS StartPublish,
    v.StopPublish AS StopPublish,
    v.CreatedBy AS CreatedBy,
    v.PublishedBy AS PublishedBy{masterInvariantCols}{masterCultureCols}
FROM ContentRoots r
JOIN {model.VersionTableName} v ON v.RootId = r.Id
    AND {masterCurrent}
WHERE r.ContentTypeKey = '{model.ClassName}'

UNION ALL

SELECT
    r.Id AS Id,
    t.Name AS Name,
    t.Language AS Language,
    r.MasterLanguage AS MasterLanguage,
    t.VersionNumber AS VersionNumber,
    t.Created AS Created,
    t.StartPublish AS StartPublish,
    t.StopPublish AS StopPublish,
    t.CreatedBy AS CreatedBy,
    t.PublishedBy AS PublishedBy{resolvedInvariantCols}{translationCultureCols}
FROM ContentRoots r
JOIN {model.TranslationTableName} t ON t.RootId = r.Id
    AND {translationCurrent}
JOIN {model.VersionTableName} mv ON mv.Id = ({masterSourceId})
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
