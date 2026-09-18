using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cms.Framework.Infrastructure;

public sealed class ContentRootConfiguration : IEntityTypeConfiguration<ContentRoot>
{
    public void Configure(EntityTypeBuilder<ContentRoot> builder)
    {
        builder.ToTable("ContentRoots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.ContentTypeKey).IsRequired();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.ContentTypeKey);
    }
}
