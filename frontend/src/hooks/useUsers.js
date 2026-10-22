import { useCallback, useEffect, useState } from "react";
import { getUsers } from "../api/user";
import { toUserRow } from "../api/user/mappers";

const useUsers = () => {
  const [users, setUsers] = useState([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await getUsers({
        take: 100,
        sort: { field: "displayName", dir: "asc" },
      });
      const rows = r.data.map(toUserRow);
      setUsers(rows);
      setTotal(r.total);
      setError(null);
      return rows;
    } catch (e) {
      setError(e);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  return { users, total, loading, error, reload: load };
};

export default useUsers;
