import { useCallback, useEffect, useState } from "react";
import { getTeam } from "../api/teams";
import { toTeamRow } from "../api/teams/mappers";

const useTeam = (id) => {
  const [team, setTeam] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setTeam(toTeamRow(await getTeam(id)));
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    load();
  }, [load]);

  return { team, loading, error, reload: load };
};

export default useTeam;
