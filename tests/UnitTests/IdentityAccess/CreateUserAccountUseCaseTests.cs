using IdentityAccess.Application;
using IdentityAccess.Application.Abstractions;
using IdentityAccess.Application.UserAccounts;
using IdentityAccess.Domain.UserAccounts;

namespace UnitTests.IdentityAccess;

/// <summary>
/// Unit test cho use case tạo Tài khoản người dùng.
/// </summary>
public sealed class CreateUserAccountUseCaseTests
{
    [Fact]
    public async Task HandleAsync_WithValidAdminRequest_CreatesAccountWithHashedPassword()
    {
        var repository = new FakeUserAccountRepository();
        var hasher = new FakeCredentialHasher();
        var useCase = new CreateUserAccountUseCase(repository, hasher);
        var request = new CreateUserAccountAppRequest("admin1", "admin1@example.com", "Admin 1", "SystemAdministrator", "Password123!");

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Created, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal("admin1", result.Value!.UserName);
        Assert.Equal("SystemAdministrator", result.Value.Role);
        Assert.NotNull(repository.AddedAccount);
        Assert.Equal("HASHED:Password123!", repository.AddedAccount!.PasswordHash);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_WithMissingPasswordForAdmin_ReturnsValidationError()
    {
        var useCase = new CreateUserAccountUseCase(new FakeUserAccountRepository(), new FakeCredentialHasher());
        var request = new CreateUserAccountAppRequest("admin1", "admin1@example.com", "Admin 1", "BusinessOperator", null);

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains("password", result.ValidationErrors!.Keys);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateUserName_ReturnsConflict()
    {
        var repository = new FakeUserAccountRepository { UserNameExists = true };
        var useCase = new CreateUserAccountUseCase(repository, new FakeCredentialHasher());
        var request = new CreateUserAccountAppRequest("admin1", "admin1@example.com", "Admin 1", "SystemAdministrator", "Password123!");

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task HandleAsync_WithIntegrationClientRole_AllowsMissingPassword()
    {
        var repository = new FakeUserAccountRepository();
        var useCase = new CreateUserAccountUseCase(repository, new FakeCredentialHasher());
        var request = new CreateUserAccountAppRequest("client1", "client1@example.com", "Client 1", "IntegrationClient", null);

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Created, result.Status);
        Assert.Null(repository.AddedAccount!.PasswordHash);
    }

    private sealed class FakeCredentialHasher : ICredentialHasher
    {
        public string Hash(string plainText) => $"HASHED:{plainText}";
        public bool Verify(string hashedValue, string plainText) => hashedValue == $"HASHED:{plainText}";
    }

    private sealed class FakeUserAccountRepository : IUserAccountRepository
    {
        public bool UserNameExists { get; set; }
        public bool EmailExists { get; set; }
        public UserAccount? AddedAccount { get; private set; }
        public int SaveChangesCallCount { get; private set; }

        public Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<UserAccount?>(null);
        public Task<UserAccount?> GetByUserNameAsync(string userName, CancellationToken cancellationToken) => Task.FromResult<UserAccount?>(null);
        public Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken) => Task.FromResult<UserAccount?>(null);
        public Task<(IReadOnlyList<UserAccount> Items, int TotalCount)> GetPagedAsync(UserAccountStatus? status, UserRole? role, int page, int pageSize, CancellationToken cancellationToken) => Task.FromResult(((IReadOnlyList<UserAccount>)Array.Empty<UserAccount>(), 0));
        public Task<bool> ExistsByUserNameAsync(string userName, Guid? excludeId, CancellationToken cancellationToken) => Task.FromResult(UserNameExists);
        public Task<bool> ExistsByEmailAsync(string email, Guid? excludeId, CancellationToken cancellationToken) => Task.FromResult(EmailExists);
        public Task AddAsync(UserAccount account, CancellationToken cancellationToken) { AddedAccount = account; return Task.CompletedTask; }
        public Task SaveChangesAsync(CancellationToken cancellationToken) { SaveChangesCallCount++; return Task.CompletedTask; }
    }
}
