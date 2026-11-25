using IdentityAccess.Application;
using IdentityAccess.Application.Abstractions;
using IdentityAccess.Application.UserAccounts;
using IdentityAccess.Domain.UserAccounts;

namespace UnitTests.IdentityAccess;

/// <summary>
/// Unit test cho use case đăng nhập credential nội bộ.
/// </summary>
public sealed class LoginUseCaseTests
{
    [Fact]
    public async Task HandleAsync_WithValidCredentials_ReturnsToken()
    {
        var account = UserAccount.Create(Guid.NewGuid(), "admin", "admin@example.com", "Admin", UserRole.SystemAdministrator, "HASHED:Password123!");
        var useCase = new LoginUseCase(new FakeUserAccountRepository(account), new FakeCredentialHasher(), new FakeJwtTokenIssuer());

        var result = await useCase.HandleAsync(new LoginAppRequest("admin", "Password123!"), CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Ok, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal("token-value", result.Value!.AccessToken);
    }

    [Fact]
    public async Task HandleAsync_WithWrongPassword_ReturnsUnauthorized()
    {
        var account = UserAccount.Create(Guid.NewGuid(), "admin", "admin@example.com", "Admin", UserRole.SystemAdministrator, "HASHED:Password123!");
        var useCase = new LoginUseCase(new FakeUserAccountRepository(account), new FakeCredentialHasher(), new FakeJwtTokenIssuer());

        var result = await useCase.HandleAsync(new LoginAppRequest("admin", "wrong"), CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task HandleAsync_WithIntegrationClientRole_ReturnsUnauthorized()
    {
        var account = UserAccount.Create(Guid.NewGuid(), "client", "client@example.com", "Client", UserRole.IntegrationClient, "HASHED:Password123!");
        var useCase = new LoginUseCase(new FakeUserAccountRepository(account), new FakeCredentialHasher(), new FakeJwtTokenIssuer());

        var result = await useCase.HandleAsync(new LoginAppRequest("client", "Password123!"), CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task HandleAsync_WithLockedAccount_ReturnsUnauthorized()
    {
        var account = UserAccount.Create(Guid.NewGuid(), "admin", "admin@example.com", "Admin", UserRole.SystemAdministrator, "HASHED:Password123!");
        account.Lock();
        var useCase = new LoginUseCase(new FakeUserAccountRepository(account), new FakeCredentialHasher(), new FakeJwtTokenIssuer());

        var result = await useCase.HandleAsync(new LoginAppRequest("admin", "Password123!"), CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Unauthorized, result.Status);
    }

    private sealed class FakeJwtTokenIssuer : IJwtTokenIssuer
    {
        public JwtTokenResult IssueAccessToken(string subject, string role, IReadOnlyDictionary<string, string>? additionalClaims = null)
            => new("token-value", "Bearer", 3600);
    }

    private sealed class FakeCredentialHasher : ICredentialHasher
    {
        public string Hash(string plainText) => $"HASHED:{plainText}";
        public bool Verify(string hashedValue, string plainText) => hashedValue == $"HASHED:{plainText}";
    }

    private sealed class FakeUserAccountRepository : IUserAccountRepository
    {
        private readonly UserAccount _account;
        public FakeUserAccountRepository(UserAccount account) { _account = account; }
        public Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_account.Id == id ? _account : null);
        public Task<UserAccount?> GetByUserNameAsync(string userName, CancellationToken cancellationToken) => Task.FromResult(_account.UserName == userName ? _account : null);
        public Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken) => Task.FromResult(_account.Email == email ? _account : null);
        public Task<(IReadOnlyList<UserAccount> Items, int TotalCount)> GetPagedAsync(UserAccountStatus? status, UserRole? role, int page, int pageSize, CancellationToken cancellationToken) => Task.FromResult(((IReadOnlyList<UserAccount>)Array.Empty<UserAccount>(), 0));
        public Task<bool> ExistsByUserNameAsync(string userName, Guid? excludeId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> ExistsByEmailAsync(string email, Guid? excludeId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task AddAsync(UserAccount account, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
