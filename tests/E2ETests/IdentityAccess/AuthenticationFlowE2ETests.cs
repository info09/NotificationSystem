using Delivery.Infrastructure.Persistence;
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
/// E2E test cho luồng xác thực cơ bản.
/// </summary>
public sealed class AuthenticationFlowE2ETests : IAsyncLifetime
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
    /// Kiểm tra endpoint gốc trả về OK.
    /// </summary>
    [Fact]
    public async Task RootEndpoint_AnonymousRequest_ReturnsOk()
    {
        var response = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Kiểm tra endpoint health trả về OK.
    /// </summary>
    [Fact]
    public async Task HealthEndpoints_AnonymousRequest_ReturnsOk()
    {
        var healthResponse = await _client.GetAsync("/health");
        var readyResponse = await _client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, readyResponse.StatusCode);
    }

    /// <summary>
    /// Kiểm tra endpoint đăng nhập đã được triển khai.
    /// </summary>
    [Fact]
    public async Task LoginEndpoint_WhenImplemented_IsNotNotFound()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            userName = "admin",
            password = "password"
        });

        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
