using Delivery.Infrastructure.Persistence;
using IdentityAccess.Application.Abstractions;
using IdentityAccess.Application.UserAccounts;
using IdentityAccess.Domain.PasswordResetTokens;
using IdentityAccess.Domain.UserAccounts;
using IdentityAccess.Infrastructure.Persistence;
using IntegrationMessaging.Infrastructure.Persistence;
using IntegrationTests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notification.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;

namespace E2ETests.IdentityAccess;

/// <summary>
/// E2E test cho luồng quên/đặt lại mật khẩu.
/// </summary>
public sealed class PasswordResetE2ETests : IAsyncLifetime
{
    private const string Issuer = "e2e-issuer";
    private const string Audience = "e2e-audience";
    private const string SigningKey = "e2e-test-signing-key-min-32-chars-000001";

    private readonly PostgreSqlContainer _postgreSqlContainer = TestcontainersConfiguration.CreatePostgreSqlBuilder().Build();
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    /// <summary>
    /// Khởi tạo môi trường test.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", _postgreSqlContainer.GetConnectionString());
            builder.UseSetting("JWT_ISSUER", Issuer);
            builder.UseSetting("JWT_AUDIENCE", Audience);
            builder.UseSetting("JWT_SIGNING_KEY", SigningKey);
        });

        using (var scope = _factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<NotificationDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<IntegrationMessagingDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<DeliveryDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>().Database.MigrateAsync();
        }

        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Giải phóng tài nguyên test.
    /// </summary>
    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }

    /// <summary>
    /// Kiểm tra luồng yêu cầu và xác nhận đặt lại mật khẩu.
    /// </summary>
    [Fact]
    public async Task RequestAndConfirmPasswordReset_WithValidToken_AllowsNewPasswordLogin()
    {
        await SeedUserAsync("e2e-admin", "e2e-admin@example.com", "E2E Admin", "Password123!");
        var rawToken = "e2e-reset-token";
        await SeedTokenAsync("e2e-admin", rawToken);

        var requestResponse = await _client.PostAsJsonAsync("/api/auth/password-reset/request", new { identifier = "e2e-admin@example.com" });
        Assert.Equal(HttpStatusCode.OK, requestResponse.StatusCode);

        var confirmResponse = await _client.PostAsJsonAsync("/api/auth/password-reset/confirm", new { token = rawToken, newPassword = "NewPassword123!" });
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { userName = "e2e-admin", password = "NewPassword123!" });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    private async Task SeedUserAsync(string userName, string email, string displayName, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<ICredentialHasher>();
        var account = UserAccount.Create(Guid.NewGuid(), userName, email, displayName, UserRole.SystemAdministrator, hasher.Hash(password));
        dbContext.UserAccounts.Add(account);
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedTokenAsync(string userName, string rawToken)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
        var account = await dbContext.UserAccounts.SingleAsync(x => x.UserName == userName);
        var token = PasswordResetToken.Create(Guid.NewGuid(), account.Id, PasswordResetTokenHashing.Hash(rawToken), DateTime.UtcNow.AddMinutes(30));
        dbContext.PasswordResetTokens.Add(token);
        await dbContext.SaveChangesAsync();
    }
}
