using System.Security.Cryptography;
using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.PasswordResetTokens;
using IdentityAccess.Domain.UserAccounts;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Use case yêu cầu đặt lại mật khẩu.
/// </summary>
public sealed class RequestPasswordResetUseCase
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IPasswordResetEmailSender _emailSender;
    private readonly IPasswordResetLinkFactory _linkFactory;

    /// <summary>
    /// Khởi tạo use case yêu cầu đặt lại mật khẩu.
    /// </summary>
    /// <param name="userAccountRepository">Repository tài khoản người dùng.</param>
    /// <param name="passwordResetTokenRepository">Repository mã đặt lại mật khẩu.</param>
    /// <param name="emailSender">Dịch vụ gửi email đặt lại mật khẩu.</param>
    /// <param name="linkFactory">Dịch vụ tạo liên kết đặt lại mật khẩu.</param>
    public RequestPasswordResetUseCase(
        IUserAccountRepository userAccountRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IPasswordResetEmailSender emailSender,
        IPasswordResetLinkFactory linkFactory)
    {
        _userAccountRepository = userAccountRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailSender = emailSender;
        _linkFactory = linkFactory;
    }

    /// <summary>
    /// Xử lý yêu cầu đặt lại mật khẩu.
    /// </summary>
    /// <param name="request">Yêu cầu đặt lại mật khẩu.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả tiếp nhận yêu cầu.</returns>
    public async Task<IdentityAccessAppResult<RequestPasswordResetResponse>> HandleAsync(
        RequestPasswordResetAppRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Identifier))
        {
            return IdentityAccessAppResult<RequestPasswordResetResponse>.Validation(new Dictionary<string, string[]>
            {
                ["identifier"] = ["Identifier is required."]
            });
        }

        var identifier = request.Identifier.Trim();
        var account = await _userAccountRepository.GetByEmailAsync(identifier, cancellationToken)
            ?? await _userAccountRepository.GetByUserNameAsync(identifier, cancellationToken);

        if (account is null ||
            account.Status != UserAccountStatus.Active ||
            (account.Role != UserRole.SystemAdministrator && account.Role != UserRole.BusinessOperator))
        {
            return IdentityAccessAppResult<RequestPasswordResetResponse>.Ok(new RequestPasswordResetResponse(true));
        }

        var utcNow = DateTime.UtcNow;
        var activeTokens = await _passwordResetTokenRepository.GetActiveByUserAccountIdAsync(account.Id, utcNow, cancellationToken);
        if (activeTokens.Any(x => x.CreatedAtUtc > utcNow.AddSeconds(-60)))
        {
            return IdentityAccessAppResult<RequestPasswordResetResponse>.Ok(new RequestPasswordResetResponse(true));
        }

        foreach (var activeToken in activeTokens)
        {
            activeToken.MarkUsed(utcNow);
        }

        var rawToken = GenerateToken();
        var tokenHash = PasswordResetTokenHashing.Hash(rawToken);
        var expiresAtUtc = utcNow.Add(_linkFactory.GetTokenLifetime());
        var resetToken = PasswordResetToken.Create(Guid.NewGuid(), account.Id, tokenHash, expiresAtUtc, utcNow);

        await _passwordResetTokenRepository.AddAsync(resetToken, cancellationToken);
        await _passwordResetTokenRepository.SaveChangesAsync(cancellationToken);

        var resetLink = _linkFactory.CreateResetLink(rawToken);
        try
        {
            await _emailSender.SendPasswordResetEmailAsync(account.Email, account.DisplayName, resetLink, cancellationToken);
        }
        catch
        {
            // Không lộ thông tin nhạy cảm; yêu cầu vẫn được tiếp nhận.
        }

        return IdentityAccessAppResult<RequestPasswordResetResponse>.Ok(new RequestPasswordResetResponse(true));
    }

    /// <summary>
    /// Sinh token đặt lại mật khẩu ngẫu nhiên.
    /// </summary>
    /// <returns>Token plaintext.</returns>
    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "-", StringComparison.Ordinal).Replace("/", "_", StringComparison.Ordinal).TrimEnd('=');
    }
}
