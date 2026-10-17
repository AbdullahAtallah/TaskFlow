import { useCallback, useEffect, useState } from "react";
import { getProjects } from "../api/projects";
import { toProjectRow } from "../api/projects/mappers";

const useTeamProjects = (teamId) => {
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await getProjects({
        teamId,
        take: 100,
        sort: { field: "name", dir: "asc" },
      });

      setProjects(
        res.data
          .map(toProjectRow)
          .filter((p) => !p.teamId || p.teamId === teamId),
      );
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }, [teamId]);

  useEffect(() => {
    load();
  }, [load]);

  return { projects, loading, error, reload: load };
};

export default useTeamProjects;
