using IdentityAccess.Application;
using IdentityAccess.Application.Abstractions;
using IdentityAccess.Application.EndUserProfiles;
using IdentityAccess.Domain.EndUserProfiles;

namespace UnitTests.IdentityAccess;

/// <summary>
/// Unit test cho use case Hồ sơ người nhận EndUser.
/// </summary>
public sealed class EndUserProfileUseCaseTests
{
    /// <summary>
    /// Kiểm tra tạo hồ sơ hợp lệ trả về Created.
    /// </summary>
    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        var repository = new FakeEndUserProfileRepository();
        var useCase = new CreateEndUserProfileUseCase(repository);

        var result = await useCase.HandleAsync(
            new CreateEndUserProfileAppRequest("ext-1", "User 1", "u1@example.com", "0123"),
            CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Created, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal("ext-1", result.Value!.ExternalUserId);
        Assert.Single(repository.Items);
    }

    /// <summary>
    /// Kiểm tra tạo hồ sơ trùng định danh nguồn trả về Conflict.
    /// </summary>
    [Fact]
    public async Task Create_WithDuplicateExternalUserId_ReturnsConflict()
    {
        var existing = EndUserProfile.Create(Guid.NewGuid(), "ext-1", "User 1", null, null);
        var repository = new FakeEndUserProfileRepository(existing);
        var useCase = new CreateEndUserProfileUseCase(repository);

        var result = await useCase.HandleAsync(
            new CreateEndUserProfileAppRequest("ext-1", "User 2", null, null),
            CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Conflict, result.Status);
    }

    /// <summary>
    /// Kiểm tra tra cứu theo định danh nguồn không tồn tại trả về NotFound.
    /// </summary>
    [Fact]
    public async Task GetByExternalUserId_WhenMissing_ReturnsNotFound()
    {
        var repository = new FakeEndUserProfileRepository();
        var useCase = new GetEndUserProfileByExternalUserIdUseCase(repository);

        var result = await useCase.HandleAsync("missing", CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.NotFound, result.Status);
    }

    /// <summary>
    /// Kiểm tra khóa hồ sơ trả về trạng thái Locked.
    /// </summary>
    [Fact]
    public async Task Lock_WithExistingProfile_ReturnsLockedProfile()
    {
        var profile = EndUserProfile.Create(Guid.NewGuid(), "ext-1", "User 1", null, null);
        var repository = new FakeEndUserProfileRepository(profile);
        var useCase = new LockEndUserProfileUseCase(repository);

        var result = await useCase.HandleAsync(profile.Id, CancellationToken.None);

        Assert.Equal(IdentityAccessAppStatus.Ok, result.Status);
        Assert.Equal("Locked", result.Value!.Status);
    }

    /// <summary>
    /// Repository giả lập cho test use case Hồ sơ người nhận EndUser.
    /// </summary>
    private sealed class FakeEndUserProfileRepository : IEndUserProfileRepository
    {
        /// <summary>
        /// Khởi tạo repository giả lập.
        /// </summary>
        /// <param name="profiles">Danh sách hồ sơ ban đầu.</param>
        public FakeEndUserProfileRepository(params EndUserProfile[] profiles)
        {
            Items = profiles.ToList();
        }

        /// <summary>
        /// Danh sách hồ sơ giả lập.
        /// </summary>
        public List<EndUserProfile> Items { get; }

        /// <inheritdoc />
        public Task<EndUserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

        /// <inheritdoc />
        public Task<EndUserProfile?> GetByExternalUserIdAsync(string externalUserId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.FirstOrDefault(x => x.ExternalUserId == externalUserId));

        /// <inheritdoc />
        public Task<(IReadOnlyList<EndUserProfile> Items, int TotalCount)> GetPagedAsync(
            EndUserProfileStatus? status,
            string? externalUserId,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            IEnumerable<EndUserProfile> query = Items;
            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(externalUserId))
            {
                query = query.Where(x => x.ExternalUserId == externalUserId.Trim());
            }

            var list = query.ToList();
            return Task.FromResult(((IReadOnlyList<EndUserProfile>)list, list.Count));
        }

        /// <inheritdoc />
        public Task<bool> ExistsByExternalUserIdAsync(string externalUserId, Guid? excludeId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.Any(x => x.ExternalUserId == externalUserId && (!excludeId.HasValue || x.Id != excludeId.Value)));

        /// <inheritdoc />
        public Task AddAsync(EndUserProfile profile, CancellationToken cancellationToken)
        {
            Items.Add(profile);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
