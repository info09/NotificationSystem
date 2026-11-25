using IdentityAccess.Domain.EndUserProfiles;
using IdentityAccess.Domain.IntegrationClientCredentials;
using IdentityAccess.Domain.PasswordResetTokens;
using IdentityAccess.Domain.UserAccounts;
using Microsoft.EntityFrameworkCore;

namespace IdentityAccess.Infrastructure.Persistence;

/// <summary>
/// DbContext của module IdentityAccess.
/// </summary>
/// <param name="options">Tùy chọn cấu hình DbContext.</param>
public sealed class IdentityAccessDbContext(
    DbContextOptions<IdentityAccessDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Tập thực thể Tài khoản người dùng.
    /// </summary>
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    /// <summary>
    /// Tập thực thể Thông tin xác thực IntegrationClient.
    /// </summary>
    public DbSet<IntegrationClientCredential> IntegrationClientCredentials => Set<IntegrationClientCredential>();

    /// <summary>
    /// Tập thực thể Mã đặt lại mật khẩu.
    /// </summary>
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    /// <summary>
    /// Tập thực thể Hồ sơ người nhận EndUser.
    /// </summary>
    public DbSet<EndUserProfile> EndUserProfiles => Set<EndUserProfile>();

    /// <summary>
    /// Cấu hình mô hình dữ liệu cho module IdentityAccess.
    /// </summary>
    /// <param name="modelBuilder">Builder cấu hình mô hình EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("identity_access");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityAccessDbContext).Assembly);
    }
}
