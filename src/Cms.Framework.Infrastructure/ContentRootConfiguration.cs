using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cms.Framework.Infrastructure;

public sealed class ContentRootConfiguration<TContentType> : IEntityTypeConfiguration<ContentRoot<TContentType>>
    where TContentType : struct, Enum
{
    public void Configure(EntityTypeBuilder<ContentRoot<TContentType>> builder)
    {
        builder.ToTable("ContentRoots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired();
        // Stored by name, not number, so adding/removing/reordering content
        // types never changes what an existing row means.
        builder.Property(x => x.ContentTypeKey).HasConversion<string>().IsRequired();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.ContentTypeKey);
    }
}
