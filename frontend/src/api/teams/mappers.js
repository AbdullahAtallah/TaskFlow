export const toTeamRow = (team) => ({
  id: team.id,
  name: team.name,
  description: team.description,
  members: team.memberCount,
  projects: team.projectCount,
  created: team.dateTimeCreated,
  rowVersion: team.rowVersion,
});
