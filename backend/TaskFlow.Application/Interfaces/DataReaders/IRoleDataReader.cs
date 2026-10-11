using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Queries.Roles;

namespace TaskFlow.Application.Interfaces.DataReaders;

public interface IRoleDataReader
{
    Task<FilteringResult<RoleRowDto>> GetGridAsync(GetRolesQuery query);

    Task<RoleDetailDto> GetByIdAsync(Guid id);

    Task<FilteringResult<RoleUserDto>> GetUsersAsync(Guid roleId, GetRoleUsersQuery query);

    Task<bool> IsNameInUseAsync(string name, Guid? excludeRoleId);

    Task<bool> HasUsersAsync(Guid roleId);

    /// <summary>
    /// The role a registering user gets: the default role. Exactly one role should be the default; should two
    /// administrators changing roles at the same moment break that, the first default role by name, or else the
    /// built-in User role, which is what the seeder restores on the next start. A role that grants Management is
    /// never given this way.
    /// </summary>
    Task<Guid> GetRoleIdForNewUsersAsync();
}
