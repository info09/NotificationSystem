using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.UserAccounts;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Use case truy vấn danh sách Tài khoản người dùng.
/// </summary>
public sealed class GetUserAccountsUseCase
{
    private readonly IUserAccountRepository _userAccountRepository;

    /// <summary>
    /// Khởi tạo use case truy vấn danh sách Tài khoản người dùng.
    /// </summary>
    /// <param name="userAccountRepository">Repository tài khoản người dùng.</param>
    public GetUserAccountsUseCase(IUserAccountRepository userAccountRepository)
    {
        _userAccountRepository = userAccountRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu truy vấn danh sách Tài khoản người dùng.
    /// </summary>
    /// <param name="request">Yêu cầu truy vấn.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả truy vấn danh sách.</returns>
    public async Task<IdentityAccessAppResult<GetUserAccountsResponse>> HandleAsync(
        GetUserAccountsAppRequest request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        if (pageSize > 100)
        {
            errors["pageSize"] = ["PageSize must be between 1 and 100."];
        }

        UserAccountStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<UserAccountStatus>(request.Status, true, out var parsedStatus))
            {
                status = parsedStatus;
            }
            else
            {
                errors["status"] = ["Status is invalid."];
            }
        }

        UserRole? role = null;
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
            {
                role = parsedRole;
            }
            else
            {
                errors["role"] = ["Role is invalid."];
            }
        }

        if (errors.Count > 0)
        {
            return IdentityAccessAppResult<GetUserAccountsResponse>.Validation(errors);
        }

        var (items, totalCount) = await _userAccountRepository.GetPagedAsync(status, role, page, pageSize, cancellationToken);
        var response = new GetUserAccountsResponse(
            items.Select(UserAccountMapping.ToDto).ToList(),
            page,
            pageSize,
            totalCount);

        return IdentityAccessAppResult<GetUserAccountsResponse>.Ok(response);
    }
}
