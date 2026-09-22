using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cms.Framework.Infrastructure.Settings;

public sealed class CmsSettingEntryConfiguration : IEntityTypeConfiguration<CmsSettingEntry>
{
    public void Configure(EntityTypeBuilder<CmsSettingEntry> builder)
    {
        builder.ToTable("CmsSettings");
        builder.HasKey(x => x.Key);
        builder.Property(x => x.Value).IsRequired();
    }
}
