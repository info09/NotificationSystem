using IdentityAccess.Application;
using IdentityAccess.Application.Abstractions;
using IdentityAccess.Application.UserAccounts;
using IdentityAccess.Domain.IntegrationClientCredentials;
using IdentityAccess.Domain.UserAccounts;

namespace UnitTests.IdentityAccess;

/// <summary>
/// Unit test cho use case cấp JWT bằng client credentials.
/// </summary>
public sealed class IssueClientCredentialsTokenUseCaseTests
{
    [Fact]
    public async Task HandleAsync_WithValidCredentials_ReturnsToken()
    {
        var account = UserAccount.Create(Guid.NewGuid(), "client", "client@example.com", "Client", UserRole.IntegrationClient, null);
        var credential = IntegrationClientCredential.Create(Guid.NewGuid(), account.Id, "client-1", "HASHED:secret");
        var useCase = new IssueClientCredentialsTokenUseCase(
            new FakeCredentialRepository(credential),
            new FakeUserAccountRepository(account),
            new FakeCredentialHasher(),
            new FakeJwtTokenIssuer());

        var result = await useCase.HandleAsync(new IssueClientCredentialsTokenAppRequest("client-1", "secret"), CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Ok, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal("token-value", result.Value!.AccessToken);
        Assert.Equal("Bearer", result.Value.TokenType);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidSecret_ReturnsUnauthorized()
    {
        var account = UserAccount.Create(Guid.NewGuid(), "client", "client@example.com", "Client", UserRole.IntegrationClient, null);
        var credential = IntegrationClientCredential.Create(Guid.NewGuid(), account.Id, "client-1", "HASHED:secret");
        var useCase = new IssueClientCredentialsTokenUseCase(
            new FakeCredentialRepository(credential),
            new FakeUserAccountRepository(account),
            new FakeCredentialHasher(),
            new FakeJwtTokenIssuer());

        var result = await useCase.HandleAsync(new IssueClientCredentialsTokenAppRequest("client-1", "wrong"), CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task HandleAsync_WithRevokedCredential_ReturnsUnauthorized()
    {
        var account = UserAccount.Create(Guid.NewGuid(), "client", "client@example.com", "Client", UserRole.IntegrationClient, null);
        var credential = IntegrationClientCredential.Create(Guid.NewGuid(), account.Id, "client-1", "HASHED:secret");
        credential.Revoke();
        var useCase = new IssueClientCredentialsTokenUseCase(
            new FakeCredentialRepository(credential),
            new FakeUserAccountRepository(account),
            new FakeCredentialHasher(),
            new FakeJwtTokenIssuer());

        var result = await useCase.HandleAsync(new IssueClientCredentialsTokenAppRequest("client-1", "secret"), CancellationToken.None);

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
        public Task<(IReadOnlyList<UserAccount> Items, int TotalCount)> GetPagedAsync(UserAccountStatus? status, UserRole? role, int page, int pageSize, CancellationToken cancellationToken) => Task.FromResult(((IReadOnlyList<UserAccount>)Array.Empty<UserAccount>(), 0));
        public Task<bool> ExistsByUserNameAsync(string userName, Guid? excludeId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> ExistsByEmailAsync(string email, Guid? excludeId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task AddAsync(UserAccount account, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeCredentialRepository : IIntegrationClientCredentialRepository
    {
        private readonly IntegrationClientCredential _credential;
        public FakeCredentialRepository(IntegrationClientCredential credential) { _credential = credential; }
        public Task<IntegrationClientCredential?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_credential.Id == id ? _credential : null);
        public Task<IntegrationClientCredential?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken) => Task.FromResult(_credential.ClientId == clientId ? _credential : null);
        public Task<IReadOnlyList<IntegrationClientCredential>> GetByUserAccountIdAsync(Guid userAccountId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<IntegrationClientCredential>>(Array.Empty<IntegrationClientCredential>());
        public Task<bool> ExistsByClientIdAsync(string clientId, Guid? excludeId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task AddAsync(IntegrationClientCredential credential, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
