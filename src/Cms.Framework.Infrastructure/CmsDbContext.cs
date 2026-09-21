using Cms.Framework.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cms.Framework.Infrastructure;

internal sealed class ContentReferenceConverter<TContentType> : ValueConverter<ContentReference<TContentType>, string>
    where TContentType : struct, Enum
{
    public ContentReferenceConverter()
        : base(r => r.ToString(), s => ContentReference<TContentType>.Parse(s))
    {
    }
}

/// <summary>
/// Generic EF Core context shared by every content type. Knows nothing
/// about any concrete content type - discovers them at model-build time
/// from whichever <see cref="IContentTypeMetadata{TContentType}"/> implementations are
/// registered in DI (one per type, contributed by generated code), and
/// applies each type's generated <c>IEntityTypeConfiguration&lt;&gt;</c>
/// via <see cref="ModelBuilder.ApplyConfigurationsFromAssembly"/>.
/// <typeparamref name="TContentType"/> is the enum the source generator emits.
/// </summary>
public sealed class CmsDbContext<TContentType> : DbContext where TContentType : struct, Enum
{
    private readonly IEnumerable<IContentTypeMetadata<TContentType>> _contentTypes;

    public CmsDbContext(DbContextOptions<CmsDbContext<TContentType>> options, IEnumerable<IContentTypeMetadata<TContentType>> contentTypes)
        : base(options)
    {
        _contentTypes = contentTypes;
    }

    public DbSet<ContentRoot<TContentType>> ContentRoots => Set<ContentRoot<TContentType>>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Applies to every generated entity, so the generator needs no
        // per-property knowledge of ContentReference.
        configurationBuilder.Properties<ContentReference<TContentType>>()
            .HaveConversion<ContentReferenceConverter<TContentType>>()
            .HaveMaxLength(256);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ContentRootConfiguration<TContentType>());

        foreach (var assembly in _contentTypes.Select(m => m.ClrType.Assembly).Distinct())
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }
    }
}
