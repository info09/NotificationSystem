using IdentityAccess.Domain.PasswordResetTokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityAccess.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho thực thể Mã đặt lại mật khẩu.
/// </summary>
public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    /// <summary>
    /// Cấu hình bảng, cột, chuyển đổi và chỉ mục cho Mã đặt lại mật khẩu.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(x => x.UserAccountId).HasColumnName("UserAccountId").IsRequired();
        builder.Property(x => x.TokenHash).HasColumnName("TokenHash").HasMaxLength(512).IsRequired();
        builder.Property(x => x.ExpiresAtUtc).HasColumnName("ExpiresAtUtc").IsRequired();
        builder.Property(x => x.UsedAtUtc).HasColumnName("UsedAtUtc");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CreatedAtUtc").IsRequired();

        builder.HasIndex(x => x.UserAccountId);
        builder.HasIndex(x => x.TokenHash).IsUnique();
    }
}
