export const toProjectRow = (p) => ({
  id: p.id,
  name: p.name,
  description: p.description,
  teamId: p.teamId,
  teamName: p.teamName,
  total: p.taskCount,
  open: p.openTaskCount,
  done: p.doneTaskCount,
  hasOverdue: p.hasOverdueTasks,
  created: p.dateTimeCreated,
  rowVersion: p.rowVersion,
});
