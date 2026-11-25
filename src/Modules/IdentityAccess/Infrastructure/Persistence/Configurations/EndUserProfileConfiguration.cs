using IdentityAccess.Domain.EndUserProfiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityAccess.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho thực thể Hồ sơ người nhận EndUser.
/// </summary>
public sealed class EndUserProfileConfiguration : IEntityTypeConfiguration<EndUserProfile>
{
    /// <summary>
    /// Cấu hình bảng, cột, chuyển đổi và chỉ mục cho Hồ sơ người nhận EndUser.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<EndUserProfile> builder)
    {
        builder.ToTable("EndUserProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(x => x.ExternalUserId).HasColumnName("ExternalUserId").HasMaxLength(128).IsRequired();
        builder.Property(x => x.DisplayName).HasColumnName("DisplayName").HasMaxLength(256).IsRequired();
        builder.Property(x => x.Email).HasColumnName("Email").HasMaxLength(256);
        builder.Property(x => x.PhoneNumber).HasColumnName("PhoneNumber").HasMaxLength(64);
        builder.Property(x => x.Status).HasColumnName("Status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CreatedAtUtc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("UpdatedAtUtc").IsRequired();

        builder.HasIndex(x => x.ExternalUserId).IsUnique();
    }
}
