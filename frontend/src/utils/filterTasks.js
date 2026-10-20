const statusValue = {
  "To do": "ToDo",
  "In progress": "InProgress",
  Done: "Done",
};

const filterTasks = (tasks, { status, priority, assignee }, currentUserId) =>
  tasks.filter((t) => {
    if (status !== "All" && t.status !== statusValue[status]) return false;
    if (priority !== "All" && t.priority !== priority) return false;
    if (assignee === "Me" && t.assigneeId !== currentUserId) return false;
    if (assignee === "Unassigned" && t.assigneeId) return false;
    return true;
  });

export default filterTasks;
