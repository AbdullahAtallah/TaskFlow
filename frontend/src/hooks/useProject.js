import { useCallback, useEffect, useState } from "react";
import { getProject } from "../api/projects";
import { toProjectRow } from "../api/projects/mappers";

const useProject = (id) => {
  const [project, setProject] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setProject(toProjectRow(await getProject(id)));
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    load();
  }, [load]);

  return { project, loading, error, reload: load };
};

export default useProject;