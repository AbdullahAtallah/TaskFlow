import { useCallback, useEffect, useState } from "react";
import { getTeamMembers } from "../api/teams";

const useTeamMembers = (teamId) => {
  const [members, setMembers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const load = useCallback(async () => {
    if (!teamId) return;
    setLoading(true);
    setError(null);
    try {
      setMembers(await getTeamMembers(teamId));
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }, [teamId]);

  useEffect(() => {
    load();
  }, [load]);

  return { members, loading, error, reload: load };
};

export default useTeamMembers;
