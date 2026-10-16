import { useCallback, useEffect, useState } from "react";
import { getTeams } from "../api/teams";
import { toTeamRow } from "../api/teams/mappers";

const useTeams = (query) => {
  const [teams, setTeams] = useState([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await getTeams({ query, take: 100 });
      setTeams(res.data.map(toTeamRow));
      setTotal(res.total);
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }, [query]);

  useEffect(() => {
    load();
  }, [load]);

  return { teams, total, loading, error, reload: load };
};

export default useTeams;
