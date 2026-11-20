using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace IdentityAccess.Presentation;

public static class JwtBearerAuthenticationExtensions
{
    public static IServiceCollection AddIdentityAccessJwtBearerAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var issuer = configuration["JWT_ISSUER"]
            ?? configuration["Jwt:Issuer"]
            ?? configuration["JwtBearer:Issuer"];
        var audience = configuration["JWT_AUDIENCE"]
            ?? configuration["Jwt:Audience"]
            ?? configuration["JwtBearer:Audience"];
        var signingKey = configuration["JWT_SIGNING_KEY"]
            ?? configuration["Jwt:SigningKey"]
            ?? configuration["JwtBearer:SigningKey"];

        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException("JWT issuer configuration is missing. Configure 'JWT_ISSUER'.");
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("JWT audience configuration is missing. Configure 'JWT_AUDIENCE'.");
        }

        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException("JWT signing key configuration is missing. Configure 'JWT_SIGNING_KEY'.");
        }

        var signingKeyBytes = Encoding.UTF8.GetBytes(signingKey);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        return services;
    }
}
