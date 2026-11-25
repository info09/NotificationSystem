using IdentityAccess.Domain.UserAccounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityAccess.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho thực thể Tài khoản người dùng.
/// </summary>
public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    /// <summary>
    /// Cấu hình bảng, cột, chuyển đổi và chỉ mục cho Tài khoản người dùng.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("UserAccounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(x => x.UserName).HasColumnName("UserName").HasMaxLength(128).IsRequired();
        builder.Property(x => x.Email).HasColumnName("Email").HasMaxLength(256).IsRequired();
        builder.Property(x => x.DisplayName).HasColumnName("DisplayName").HasMaxLength(256).IsRequired();
        builder.Property(x => x.Role).HasColumnName("Role").HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(x => x.Status).HasColumnName("Status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnName("PasswordHash").HasMaxLength(512);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CreatedAtUtc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("UpdatedAtUtc").IsRequired();

        builder.HasIndex(x => x.UserName).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.Role);
    }
}
