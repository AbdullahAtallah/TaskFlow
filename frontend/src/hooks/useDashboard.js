import { useCallback, useEffect, useState } from "react";
import {
  getMyTasks,
  getStatusCounts,
  getProjectCountsPerTeam,
} from "../api/dashboard";
import { getTeams } from "../api/teams";
import { toTaskRow } from "../api/tasks/mappers";
import { toTeamRow } from "../api/teams/mappers";

const EMPTY = {
  myTasks: [],
  myTasksTotal: 0,
  statusCounts: { toDo: 0, inProgress: 0, done: 0, overdue: 0 },
  projectsPerTeam: [],
  myTeams: [],
};

const useDashboard = ({ canSeeTeams, canSeeProjects }) => {
  const [data, setData] = useState(EMPTY);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);

    const [myTasks, counts, perTeam, teams] = await Promise.allSettled([
      getMyTasks({ take: 10, sort: { field: "dueDate", dir: "asc" } }),
      getStatusCounts(),
      canSeeProjects ? getProjectCountsPerTeam() : Promise.resolve([]),
      canSeeTeams
        ? getTeams({
            onlyMine: true,
            take: 100,
            sort: { field: "name", dir: "asc" },
          })
        : Promise.resolve({ data: [] }),
    ]);

    setData((prev) => ({
      myTasks:
        myTasks.status === "fulfilled"
          ? myTasks.value.data.map(toTaskRow)
          : prev.myTasks,
      myTasksTotal:
        myTasks.status === "fulfilled"
          ? myTasks.value.total
          : prev.myTasksTotal,
      statusCounts:
        counts.status === "fulfilled" ? counts.value : prev.statusCounts,
      projectsPerTeam:
        perTeam.status === "fulfilled" ? perTeam.value : prev.projectsPerTeam,
      myTeams:
        teams.status === "fulfilled"
          ? teams.value.data.map(toTeamRow)
          : prev.myTeams,
    }));
    setLoading(false);
  }, [canSeeTeams, canSeeProjects]);

  useEffect(() => {
    load();
  }, [load]);

  return { ...data, loading, reload: load };
};

export default useDashboard;
