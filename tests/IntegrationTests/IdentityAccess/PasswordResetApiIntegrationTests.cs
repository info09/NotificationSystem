using System.Net;
using System.Net.Http.Json;
using Delivery.Infrastructure.Persistence;
using IdentityAccess.Application.Abstractions;
using IdentityAccess.Application.UserAccounts;
using IdentityAccess.Domain.PasswordResetTokens;
using IdentityAccess.Domain.UserAccounts;
using IdentityAccess.Infrastructure.Persistence;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notification.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace IntegrationTests.IdentityAccess;

/// <summary>
/// Integration test cho luồng đặt lại mật khẩu.
/// </summary>
public sealed class PasswordResetApiIntegrationTests : IAsyncLifetime
{
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";
    private const string SigningKey = "integration-test-signing-key-min-32-chars-0001";

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

    [Fact]
    public async Task RequestPasswordReset_WithUnknownUser_ReturnsAccepted()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/request", new { identifier = "unknown@example.com" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<RequestResponse>();
        Assert.NotNull(payload);
        Assert.True(payload!.Accepted);
    }

    [Fact]
    public async Task ConfirmPasswordReset_WithValidToken_ResetsPassword()
    {
        var userId = await SeedUserAsync("reset-admin", "reset-admin@example.com", "Reset Admin", "Password123!");
        var rawToken = "raw-reset-token";
        await SeedTokenAsync(userId, rawToken);

        var response = await _client.PostAsJsonAsync("/api/auth/password-reset/confirm", new { token = rawToken, newPassword = "NewPassword123!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ConfirmResponse>();
        Assert.NotNull(payload);
        Assert.True(payload!.Reset);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { userName = "reset-admin", password = "NewPassword123!" });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    private async Task<Guid> SeedUserAsync(string userName, string email, string displayName, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<ICredentialHasher>();
        var account = UserAccount.Create(Guid.NewGuid(), userName, email, displayName, UserRole.SystemAdministrator, hasher.Hash(password));
        dbContext.UserAccounts.Add(account);
        await dbContext.SaveChangesAsync();
        return account.Id;
    }

    private async Task SeedTokenAsync(Guid userAccountId, string rawToken)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
        var token = PasswordResetToken.Create(Guid.NewGuid(), userAccountId, PasswordResetTokenHashing.Hash(rawToken), DateTime.UtcNow.AddMinutes(30));
        dbContext.PasswordResetTokens.Add(token);
        await dbContext.SaveChangesAsync();
    }

    private sealed record RequestResponse(bool Accepted);
    private sealed record ConfirmResponse(bool Reset);
}
