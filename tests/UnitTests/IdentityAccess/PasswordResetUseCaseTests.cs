using IdentityAccess.Application;
using IdentityAccess.Application.Abstractions;
using IdentityAccess.Application.UserAccounts;
using IdentityAccess.Domain.PasswordResetTokens;
using IdentityAccess.Domain.UserAccounts;

namespace UnitTests.IdentityAccess;

/// <summary>
/// Unit test cho luồng đặt lại mật khẩu.
/// </summary>
public sealed class PasswordResetUseCaseTests
{
    [Fact]
    public async Task RequestPasswordReset_WithExistingAdmin_CreatesTokenAndSendsEmail()
    {
        var account = UserAccount.Create(Guid.NewGuid(), "admin", "admin@example.com", "Admin", UserRole.SystemAdministrator, "HASHED:old");
        var userRepo = new FakeUserAccountRepository(account);
        var tokenRepo = new FakePasswordResetTokenRepository();
        var emailSender = new FakePasswordResetEmailSender();
        var useCase = new RequestPasswordResetUseCase(userRepo, tokenRepo, emailSender, new FakePasswordResetLinkFactory());

        var result = await useCase.HandleAsync(new RequestPasswordResetAppRequest("admin@example.com"), CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Ok, result.Status);
        Assert.True(result.Value!.Accepted);
        Assert.Single(tokenRepo.Tokens);
        Assert.Equal(account.Email, emailSender.LastToEmail);
        Assert.Contains("token=", emailSender.LastResetLink);
    }

    [Fact]
    public async Task RequestPasswordReset_WithUnknownUser_ReturnsAcceptedWithoutSendingEmail()
    {
        var useCase = new RequestPasswordResetUseCase(new FakeUserAccountRepository(null), new FakePasswordResetTokenRepository(), new FakePasswordResetEmailSender(), new FakePasswordResetLinkFactory());

        var result = await useCase.HandleAsync(new RequestPasswordResetAppRequest("unknown@example.com"), CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Ok, result.Status);
        Assert.True(result.Value!.Accepted);
    }

    [Fact]
    public async Task ConfirmPasswordReset_WithValidToken_UpdatesPasswordAndMarksTokenUsed()
    {
        var account = UserAccount.Create(Guid.NewGuid(), "admin", "admin@example.com", "Admin", UserRole.SystemAdministrator, "HASHED:old");
        var rawToken = "reset-token";
        var token = PasswordResetToken.Create(Guid.NewGuid(), account.Id, PasswordResetTokenHashing.Hash(rawToken), DateTime.UtcNow.AddMinutes(30));
        var userRepo = new FakeUserAccountRepository(account);
        var tokenRepo = new FakePasswordResetTokenRepository(token);
        var useCase = new ConfirmPasswordResetUseCase(userRepo, tokenRepo, new FakeCredentialHasher());

        var result = await useCase.HandleAsync(new ConfirmPasswordResetAppRequest(rawToken, "NewPassword123!"), CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Ok, result.Status);
        Assert.True(result.Value!.Reset);
        Assert.Equal("HASHED:NewPassword123!", account.PasswordHash);
        Assert.True(token.IsUsed());
    }

    [Fact]
    public async Task ConfirmPasswordReset_WithExpiredToken_ReturnsValidationError()
    {
        var account = UserAccount.Create(Guid.NewGuid(), "admin", "admin@example.com", "Admin", UserRole.SystemAdministrator, "HASHED:old");
        var rawToken = "reset-token";
        var token = PasswordResetToken.Create(Guid.NewGuid(), account.Id, PasswordResetTokenHashing.Hash(rawToken), DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(-30));
        var useCase = new ConfirmPasswordResetUseCase(new FakeUserAccountRepository(account), new FakePasswordResetTokenRepository(token), new FakeCredentialHasher());

        var result = await useCase.HandleAsync(new ConfirmPasswordResetAppRequest(rawToken, "NewPassword123!"), CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains("token", result.ValidationErrors!.Keys);
    }

    private sealed class FakeCredentialHasher : ICredentialHasher
    {
        public string Hash(string plainText) => $"HASHED:{plainText}";
        public bool Verify(string hashedValue, string plainText) => hashedValue == $"HASHED:{plainText}";
    }

    private sealed class FakePasswordResetLinkFactory : IPasswordResetLinkFactory
    {
        public string CreateResetLink(string rawToken) => $"http://localhost:4200/reset-password?token={rawToken}";
        public TimeSpan GetTokenLifetime() => TimeSpan.FromMinutes(30);
    }

    private sealed class FakePasswordResetEmailSender : IPasswordResetEmailSender
    {
        public string? LastToEmail { get; private set; }
        public string? LastResetLink { get; private set; }
        public Task SendPasswordResetEmailAsync(string toEmail, string displayName, string resetLink, CancellationToken cancellationToken)
        {
            LastToEmail = toEmail;
            LastResetLink = resetLink;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserAccountRepository : IUserAccountRepository
    {
        private readonly UserAccount? _account;
        public FakeUserAccountRepository(UserAccount? account) { _account = account; }
        public Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_account?.Id == id ? _account : null);
        public Task<UserAccount?> GetByUserNameAsync(string userName, CancellationToken cancellationToken) => Task.FromResult(_account?.UserName == userName ? _account : null);
        public Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken) => Task.FromResult(_account?.Email == email ? _account : null);
        public Task<(IReadOnlyList<UserAccount> Items, int TotalCount)> GetPagedAsync(UserAccountStatus? status, UserRole? role, int page, int pageSize, CancellationToken cancellationToken) => Task.FromResult(((IReadOnlyList<UserAccount>)Array.Empty<UserAccount>(), 0));
        public Task<bool> ExistsByUserNameAsync(string userName, Guid? excludeId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> ExistsByEmailAsync(string email, Guid? excludeId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task AddAsync(UserAccount account, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakePasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        public List<PasswordResetToken> Tokens { get; }
        public FakePasswordResetTokenRepository(params PasswordResetToken[] tokens) { Tokens = tokens.ToList(); }
        public Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken) { Tokens.Add(token); return Task.CompletedTask; }
        public Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken) => Task.FromResult(Tokens.FirstOrDefault(x => x.TokenHash == tokenHash));
        public Task<IReadOnlyList<PasswordResetToken>> GetActiveByUserAccountIdAsync(Guid userAccountId, DateTime utcNow, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<PasswordResetToken>>(Tokens.Where(x => x.UserAccountId == userAccountId && !x.IsUsed() && !x.IsExpired(utcNow)).ToList());
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
