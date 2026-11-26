using Delivery.Application.Abstractions;
using Delivery.Application.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Infrastructure.Email;

/// <summary>
/// Phương thức mở rộng đăng ký dịch vụ Email cho module Delivery.
/// </summary>
public static class DeliveryEmailServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký cấu hình SMTP và Nhà cung cấp Email.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    /// <returns>Tập hợp dịch vụ để chuỗi cấu hình.</returns>
    public static IServiceCollection AddDeliveryEmail(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

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

        services.AddSingleton<IEmailProvider, SmtpEmailProvider>();
        services.AddScoped<IEmailTemplateRenderer, PassThroughEmailTemplateRenderer>();
        services.AddScoped<IEmailSender, EmailSender>();
        return services;
    }
}
