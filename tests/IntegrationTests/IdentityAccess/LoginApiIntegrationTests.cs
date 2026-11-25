using System.Net;
using System.Net.Http.Json;
using Delivery.Infrastructure.Persistence;
using IdentityAccess.Domain.UserAccounts;
using IdentityAccess.Infrastructure.Persistence;
using IdentityAccess.Application.Abstractions;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notification.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace IntegrationTests.IdentityAccess;

/// <summary>
/// Integration test cho endpoint đăng nhập credential nội bộ.
/// </summary>
public sealed class LoginApiIntegrationTests : IAsyncLifetime
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
    public async Task Login_WithValidCredentials_ReturnsAccessToken()
    {
        await SeedUserAsync("admin1", "admin1@example.com", "Admin 1", UserRole.SystemAdministrator, "Password123!");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { userName = "admin1", password = "Password123!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<LoginResponsePayload>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.AccessToken));
        Assert.Equal("Bearer", payload.TokenType);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        await SeedUserAsync("admin2", "admin2@example.com", "Admin 2", UserRole.SystemAdministrator, "Password123!");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { userName = "admin2", password = "wrong" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task SeedUserAsync(string userName, string email, string displayName, UserRole role, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<ICredentialHasher>();

        var account = UserAccount.Create(Guid.NewGuid(), userName, email, displayName, role, hasher.Hash(password));
        dbContext.UserAccounts.Add(account);
        await dbContext.SaveChangesAsync();
    }

    private sealed record LoginResponsePayload(string AccessToken, string TokenType, int ExpiresIn);
}
