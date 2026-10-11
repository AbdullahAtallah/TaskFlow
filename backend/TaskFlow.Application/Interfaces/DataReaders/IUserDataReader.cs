using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Queries.Users;

namespace TaskFlow.Application.Interfaces.DataReaders;

public interface IUserDataReader
{
    Task<FilteringResult<UserRowDto>> GetGridAsync(GetUsersQuery query);

    Task<UserDetailDto> GetByIdAsync(Guid id);

    Task<IReadOnlyList<UserSelectItemDto>> GetSelectListAsync(GetUserSelectListQuery query);

    Task<bool> IsUserNameInUseAsync(string userName);
}
