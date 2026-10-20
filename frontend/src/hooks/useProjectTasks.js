import { useCallback, useEffect, useState } from "react";
import { getTasks } from "../api/tasks";
import { toTaskRow } from "../api/tasks/mappers";

const useProjectTasks = (projectId) => {
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await getTasks({
        projectId,
        take: 100,
        sort: { field: "dueDate", dir: "asc" },
      });
      const rows = res.data
        .map(toTaskRow)
        .filter((t) => t.projectId === projectId);
      setTasks(rows);
      return rows;
    } catch (err) {
      setError(err);
      return [];
    } finally {
      setLoading(false);
    }
  }, [projectId]);

  useEffect(() => {
    load();
  }, [load]);

  return { tasks, loading, error, reload: load };
};

export default useProjectTasks;
