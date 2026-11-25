using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityAccess.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IdentityAccess.Infrastructure.Security;

/// <summary>
/// Dịch vụ phát hành JWT theo cấu hình hệ thống.
/// </summary>
public sealed class JwtTokenIssuer : IJwtTokenIssuer
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SigningCredentials _signingCredentials;
    private readonly int _expiresInMinutes;

    /// <summary>
    /// Khởi tạo dịch vụ phát hành JWT.
    /// </summary>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    public JwtTokenIssuer(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _issuer = configuration["JWT_ISSUER"]
            ?? configuration["Jwt:Issuer"]
            ?? configuration["JwtBearer:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer configuration is missing. Configure 'JWT_ISSUER'.");

        _audience = configuration["JWT_AUDIENCE"]
            ?? configuration["Jwt:Audience"]
            ?? configuration["JwtBearer:Audience"]
            ?? throw new InvalidOperationException("JWT audience configuration is missing. Configure 'JWT_AUDIENCE'.");

        var signingKey = configuration["JWT_SIGNING_KEY"]
            ?? configuration["Jwt:SigningKey"]
            ?? configuration["JwtBearer:SigningKey"]
            ?? throw new InvalidOperationException("JWT signing key configuration is missing. Configure 'JWT_SIGNING_KEY'.");

        _expiresInMinutes = int.TryParse(configuration["JWT_ACCESS_TOKEN_MINUTES"], out var minutes) && minutes > 0
            ? minutes
            : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    /// <inheritdoc />
    public JwtTokenResult IssueAccessToken(string subject, string role, IReadOnlyDictionary<string, string>? additionalClaims = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_expiresInMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.NameIdentifier, subject),
            new(ClaimTypes.Role, role)
        };

        if (additionalClaims is not null)
        {
            foreach (var pair in additionalClaims)
            {
                if (!string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
                {
                    claims.Add(new Claim(pair.Key, pair.Value));
                }
            }
        }

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: _signingCredentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return new JwtTokenResult(tokenValue, "Bearer", (int)TimeSpan.FromMinutes(_expiresInMinutes).TotalSeconds);
    }
}
