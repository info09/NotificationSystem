using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.EndUserProfiles;

namespace IdentityAccess.Application.EndUserProfiles;

/// <summary>
/// Use case truy vấn danh sách Hồ sơ người nhận EndUser.
/// </summary>
public sealed class GetEndUserProfilesUseCase
{
    private readonly IEndUserProfileRepository _endUserProfileRepository;

    /// <summary>
    /// Khởi tạo use case truy vấn danh sách Hồ sơ người nhận EndUser.
    /// </summary>
    /// <param name="endUserProfileRepository">Repository hồ sơ EndUser.</param>
    public GetEndUserProfilesUseCase(IEndUserProfileRepository endUserProfileRepository)
    {
        _endUserProfileRepository = endUserProfileRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu truy vấn danh sách Hồ sơ người nhận EndUser.
    /// </summary>
    /// <param name="request">Yêu cầu truy vấn.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả truy vấn danh sách.</returns>
    public async Task<IdentityAccessAppResult<GetEndUserProfilesResponse>> HandleAsync(
        GetEndUserProfilesAppRequest request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        if (pageSize > 100)
        {
            errors["pageSize"] = ["PageSize must be between 1 and 100."];
        }

        EndUserProfileStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<EndUserProfileStatus>(request.Status, true, out var parsedStatus))
            {
                status = parsedStatus;
            }
            else
            {
                errors["status"] = ["Status is invalid."];
            }
        }

        if (errors.Count > 0)
        {
            return IdentityAccessAppResult<GetEndUserProfilesResponse>.Validation(errors);
        }

        var (items, totalCount) = await _endUserProfileRepository.GetPagedAsync(
            status,
            request.ExternalUserId,
            page,
            pageSize,
            cancellationToken);

        var response = new GetEndUserProfilesResponse(
            items.Select(EndUserProfileMapping.ToDto).ToList(),
            page,
            pageSize,
            totalCount);

        return IdentityAccessAppResult<GetEndUserProfilesResponse>.Ok(response);
    }
}
