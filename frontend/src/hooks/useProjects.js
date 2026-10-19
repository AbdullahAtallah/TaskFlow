import { useCallback, useEffect, useState } from "react";
import { getProjects } from "../api/projects";
import { toProjectRow } from "../api/projects/mappers";

const useProjects = (teamId) => {
  const [projects, setProjects] = useState([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const filter = teamId === "all" ? undefined : teamId;
      const res = await getProjects({
        teamId: filter,
        take: 100,
        sort: { field: "name", dir: "asc" },
      });

      const rows = res.data
        .map(toProjectRow)
        .filter((p) => !filter || p.teamId === filter);
      setProjects(rows);
      setTotal(filter ? rows.length : res.total);
      return rows;
    } catch (err) {
      setError(err);
      return [];
    } finally {
      setLoading(false);
    }
  }, [teamId]);

  useEffect(() => {
    load();
  }, [load]);

  return { projects, total, loading, error, reload: load };
};

export default useProjects;
