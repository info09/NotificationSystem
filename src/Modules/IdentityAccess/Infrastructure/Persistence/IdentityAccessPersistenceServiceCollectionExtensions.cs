using IdentityAccess.Application.Abstractions;
using IdentityAccess.Application.UserAccounts;
using IdentityAccess.Infrastructure.Persistence.Repositories;
using IdentityAccess.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityAccess.Infrastructure.Persistence;

/// <summary>
/// Phương thức mở rộng đăng ký persistence cho module IdentityAccess.
/// </summary>
public static class IdentityAccessPersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký DbContext và các thành phần persistence của module IdentityAccess.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    /// <returns>Tập hợp dịch vụ để chuỗi cấu hình.</returns>
    public static IServiceCollection AddIdentityAccessPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("Database");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Database' is not configured.");
        }

        services.AddDbContext<IdentityAccessDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(typeof(IdentityAccessDbContext).Assembly.FullName)
                    .MigrationsHistoryTable("__EFMigrationsHistory", "identity_access")));

        services.AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>();
        services.AddScoped<ICredentialHasher, Pbkdf2CredentialHasher>();
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<IIntegrationClientCredentialRepository, IntegrationClientCredentialRepository>();

        services.AddScoped<CreateUserAccountUseCase>();
        services.AddScoped<UpdateUserAccountUseCase>();
        services.AddScoped<GetUserAccountByIdUseCase>();
        services.AddScoped<GetUserAccountsUseCase>();
        services.AddScoped<LockUserAccountUseCase>();
        services.AddScoped<UnlockUserAccountUseCase>();
        services.AddScoped<DeleteUserAccountUseCase>();
        services.AddScoped<CreateIntegrationClientCredentialUseCase>();
        services.AddScoped<GetIntegrationClientCredentialsUseCase>();
        services.AddScoped<RotateIntegrationClientCredentialUseCase>();
        services.AddScoped<RevokeIntegrationClientCredentialUseCase>();
        services.AddScoped<IssueClientCredentialsTokenUseCase>();
        services.AddScoped<LoginUseCase>();

        return services;
    }
}
