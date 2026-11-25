using IdentityAccess.Domain.IntegrationClientCredentials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityAccess.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho thực thể Thông tin xác thực IntegrationClient.
/// </summary>
public sealed class IntegrationClientCredentialConfiguration : IEntityTypeConfiguration<IntegrationClientCredential>
{
    /// <summary>
    /// Cấu hình bảng, cột, chuyển đổi và chỉ mục cho Thông tin xác thực IntegrationClient.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<IntegrationClientCredential> builder)
    {
        builder.ToTable("IntegrationClientCredentials");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(x => x.UserAccountId).HasColumnName("UserAccountId").IsRequired();
        builder.Property(x => x.ClientId).HasColumnName("ClientId").HasMaxLength(128).IsRequired();
        builder.Property(x => x.ClientSecretHash).HasColumnName("ClientSecretHash").HasMaxLength(512).IsRequired();
        builder.Property(x => x.Status).HasColumnName("Status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CreatedAtUtc").IsRequired();
        builder.Property(x => x.ExpiresAtUtc).HasColumnName("ExpiresAtUtc");
        builder.Property(x => x.RevokedAtUtc).HasColumnName("RevokedAtUtc");
        builder.Property(x => x.LastRotatedAtUtc).HasColumnName("LastRotatedAtUtc");

        builder.HasIndex(x => x.ClientId).IsUnique();
        builder.HasIndex(x => x.UserAccountId);
    }
}
