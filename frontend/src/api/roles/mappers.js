export const toRoleRow = (role) => ({
  id: role.id,
  name: role.name,
  description: role.description,
  packages: role.packageCount,
  claims: role.claimCount,
  users: role.userCount,
  isDefault: role.isDefault,
});
