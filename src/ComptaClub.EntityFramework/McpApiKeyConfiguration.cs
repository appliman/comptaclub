using ComptaClub.Datas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComptaClub.EntityFramework;

public sealed class McpApiKeyConfiguration : IEntityTypeConfiguration<McpApiKeyData>
{
    public void Configure(EntityTypeBuilder<McpApiKeyData> builder)
    {
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Name).HasMaxLength(100).IsRequired();
        builder.Property(item => item.KeyIdentifier).HasMaxLength(24).IsRequired();
        builder.Property(item => item.SecretHash).HasMaxLength(64).IsRequired();
        builder.Property(item => item.SecretLastFour).HasMaxLength(4).IsRequired();
        builder.Property(item => item.Version).IsConcurrencyToken();
        builder.HasIndex(item => item.KeyIdentifier).IsUnique();
        builder.HasIndex(item => item.CreatedByUserId);
    }
}
