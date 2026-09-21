using Cms.Framework.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cms.Framework.Infrastructure;

internal sealed class ContentReferenceConverter : ValueConverter<ContentReference, string>
{
    public ContentReferenceConverter()
        : base(r => r.ToString(), s => ContentReference.Parse(s))
    {
    }
}

/// <summary>
/// Generic EF Core context shared by every content type. Knows nothing
/// about any concrete content type - discovers them at model-build time
/// from whichever <see cref="IContentTypeMetadata"/> implementations are
/// registered in DI (one per type, contributed by generated code), and
/// applies each type's generated <c>IEntityTypeConfiguration&lt;&gt;</c>
/// via <see cref="ModelBuilder.ApplyConfigurationsFromAssembly"/>.
/// </summary>
public sealed class CmsDbContext : DbContext
{
    private readonly IEnumerable<IContentTypeMetadata> _contentTypes;

    public CmsDbContext(DbContextOptions<CmsDbContext> options, IEnumerable<IContentTypeMetadata> contentTypes)
        : base(options)
    {
        _contentTypes = contentTypes;
    }

    public DbSet<ContentRoot> ContentRoots => Set<ContentRoot>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Applies to every generated entity, so the generator needs no
        // per-property knowledge of ContentReference.
        configurationBuilder.Properties<ContentReference>()
            .HaveConversion<ContentReferenceConverter>()
            .HaveMaxLength(256);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ContentRootConfiguration());

        foreach (var assembly in _contentTypes.Select(m => m.ClrType.Assembly).Distinct())
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }
    }
}
