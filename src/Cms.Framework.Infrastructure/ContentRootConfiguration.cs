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
        // Stored by name, not number, so adding/removing/reordering content
        // types never changes what an existing row means.
        builder.Property(x => x.ContentTypeKey).HasConversion<string>().IsRequired();
        builder.Property(x => x.MasterLanguage).IsRequired();
        builder.HasIndex(x => x.ContentTypeKey);
    }
}
