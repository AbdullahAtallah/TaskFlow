import { useCallback, useEffect, useState } from "react";
import { getRole, getRoleUsers } from "../api/roles";

const useRole = (id) => {
  const [role, setRole] = useState(null);
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const load = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    try {
      const [r, u] = await Promise.all([
        getRole(id),
        getRoleUsers(id, { take: 100 }),
      ]);
      setRole(r);
      setUsers(u.data);
      setError(null);
    } catch (e) {
      setError(e);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    load();
  }, [load]);

  return { role, users, loading, error, reload: load };
};

export default useRole;
