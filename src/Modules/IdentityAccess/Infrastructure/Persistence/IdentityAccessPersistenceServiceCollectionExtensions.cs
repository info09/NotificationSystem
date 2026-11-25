using IdentityAccess.Application.Abstractions;
using IdentityAccess.Application.UserAccounts;
using IdentityAccess.Infrastructure.Email;
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

        services.AddOptions<SmtpOptions>()
            .Configure(options =>
            {
                configuration.GetSection(SmtpOptions.SectionName).Bind(options);
                options.Host = configuration["SMTP_HOST"] ?? configuration["Smtp:Host"] ?? options.Host;
                options.Username = configuration["SMTP_USERNAME"] ?? configuration["Smtp:Username"] ?? options.Username;
                options.Password = configuration["SMTP_PASSWORD"] ?? configuration["Smtp:Password"] ?? options.Password;
                options.From = configuration["SMTP_FROM"] ?? configuration["Smtp:From"] ?? options.From;

                var portValue = configuration["SMTP_PORT"] ?? configuration["Smtp:Port"];
                if (int.TryParse(portValue, out var port))
                {
                    options.Port = port;
                }

                var enableSslValue = configuration["SMTP_ENABLE_SSL"] ?? configuration["Smtp:EnableSsl"];
                if (bool.TryParse(enableSslValue, out var enableSsl))
                {
                    options.EnableSsl = enableSsl;
                }
            });

        services.AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>();
        services.AddSingleton<IPasswordResetLinkFactory, PasswordResetLinkFactory>();
        services.AddScoped<ICredentialHasher, Pbkdf2CredentialHasher>();
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<IIntegrationClientCredentialRepository, IntegrationClientCredentialRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IPasswordResetEmailSender, SmtpPasswordResetEmailSender>();

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
        services.AddScoped<RequestPasswordResetUseCase>();
        services.AddScoped<ConfirmPasswordResetUseCase>();

        return services;
    }
}
