export const toUserRow = (user) => ({
  id: user.id,
  name: user.displayName,
  email: user.userName,
  roles: user.roles.map((r) => r.name),
  teams: user.teams.map((t) => t.name).join(", "),
  active: user.isActive,
  created: user.dateTimeCreated,
  rowVersion: user.rowVersion,
});