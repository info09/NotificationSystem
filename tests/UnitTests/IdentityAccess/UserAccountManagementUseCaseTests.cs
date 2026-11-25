using IdentityAccess.Application;
using IdentityAccess.Application.Abstractions;
using IdentityAccess.Application.UserAccounts;
using IdentityAccess.Domain.IntegrationClientCredentials;
using IdentityAccess.Domain.UserAccounts;

namespace UnitTests.IdentityAccess;

/// <summary>
/// Unit test cho các use case quản trị Tài khoản người dùng.
/// </summary>
public sealed class UserAccountManagementUseCaseTests
{
    [Fact]
    public async Task LockUserAccountUseCase_WithSelfLock_ReturnsValidationError()
    {
        var account = UserAccount.Create(Guid.NewGuid(), "admin", "admin@example.com", "Admin", UserRole.SystemAdministrator, "hash");
        var repository = new FakeUserAccountRepository(account);
        var useCase = new LockUserAccountUseCase(repository);

        var result = await useCase.HandleAsync(account.Id, new IdentityAccessCallerContext(account.Id.ToString(), ["SystemAdministrator"]), CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.ValidationError, result.Status);
    }

    [Fact]
    public async Task DeleteUserAccountUseCase_WithValidAccount_MarksDeleted()
    {
        var account = UserAccount.Create(Guid.NewGuid(), "admin", "admin@example.com", "Admin", UserRole.SystemAdministrator, "hash");
        var repository = new FakeUserAccountRepository(account);
        var useCase = new DeleteUserAccountUseCase(repository);

        var result = await useCase.HandleAsync(account.Id, new IdentityAccessCallerContext(Guid.NewGuid().ToString(), ["SystemAdministrator"]), CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Ok, result.Status);
        Assert.Equal(UserAccountStatus.Deleted, account.Status);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task CreateIntegrationClientCredentialUseCase_WithIntegrationClientRole_ReturnsSecretOnce()
    {
        var account = UserAccount.Create(Guid.NewGuid(), "client", "client@example.com", "Client", UserRole.IntegrationClient, null);
        var userRepo = new FakeUserAccountRepository(account);
        var credRepo = new FakeCredentialRepository();
        var useCase = new CreateIntegrationClientCredentialUseCase(userRepo, credRepo, new FakeCredentialHasher());

        var result = await useCase.HandleAsync(account.Id, new CreateIntegrationClientCredentialAppRequest(null, null), CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Created, result.Status);
        Assert.NotNull(result.Value);
        Assert.False(string.IsNullOrWhiteSpace(result.Value!.ClientSecret));
        Assert.NotNull(credRepo.AddedCredential);
        Assert.NotEqual(result.Value.ClientSecret, credRepo.AddedCredential!.ClientSecretHash);
    }

    [Fact]
    public async Task RotateIntegrationClientCredentialUseCase_WithRevokedCredential_ReturnsValidationError()
    {
        var accountId = Guid.NewGuid();
        var credential = IntegrationClientCredential.Create(Guid.NewGuid(), accountId, "client-1", "hash");
        credential.Revoke();
        var credRepo = new FakeCredentialRepository(credential);
        var useCase = new RotateIntegrationClientCredentialUseCase(credRepo, new FakeCredentialHasher());

        var result = await useCase.HandleAsync(accountId, credential.Id, CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.ValidationError, result.Status);
    }

    private sealed class FakeCredentialHasher : ICredentialHasher
    {
        public string Hash(string plainText) => $"HASHED:{plainText}";
        public bool Verify(string hashedValue, string plainText) => hashedValue == $"HASHED:{plainText}";
    }

    private sealed class FakeUserAccountRepository : IUserAccountRepository
    {
        private readonly UserAccount? _account;
        public FakeUserAccountRepository(UserAccount? account = null) { _account = account; }
        public int SaveChangesCallCount { get; private set; }
        public Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_account?.Id == id ? _account : null);
        public Task<(IReadOnlyList<UserAccount> Items, int TotalCount)> GetPagedAsync(UserAccountStatus? status, UserRole? role, int page, int pageSize, CancellationToken cancellationToken) => Task.FromResult(((IReadOnlyList<UserAccount>)Array.Empty<UserAccount>(), 0));
        public Task<bool> ExistsByUserNameAsync(string userName, Guid? excludeId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> ExistsByEmailAsync(string email, Guid? excludeId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task AddAsync(UserAccount account, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken) { SaveChangesCallCount++; return Task.CompletedTask; }
    }

    private sealed class FakeCredentialRepository : IIntegrationClientCredentialRepository
    {
        private readonly IntegrationClientCredential? _credential;
        public FakeCredentialRepository(IntegrationClientCredential? credential = null) { _credential = credential; }
        public IntegrationClientCredential? AddedCredential { get; private set; }
        public Task<IntegrationClientCredential?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_credential?.Id == id ? _credential : null);
        public Task<IntegrationClientCredential?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken) => Task.FromResult(_credential?.ClientId == clientId ? _credential : null);
        public Task<IReadOnlyList<IntegrationClientCredential>> GetByUserAccountIdAsync(Guid userAccountId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<IntegrationClientCredential>>(Array.Empty<IntegrationClientCredential>());
        public Task<bool> ExistsByClientIdAsync(string clientId, Guid? excludeId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task AddAsync(IntegrationClientCredential credential, CancellationToken cancellationToken) { AddedCredential = credential; return Task.CompletedTask; }
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
