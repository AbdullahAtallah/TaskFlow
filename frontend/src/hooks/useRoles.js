import { useCallback, useEffect, useState } from "react";
import { getRoles } from "../api/roles";
import { toRoleRow } from "../api/roles/mappers";

const ORDER = ["Admin", "User", "ReadOnlyUser"];
const byOrder = (a, b) =>
  (ORDER.indexOf(a.name) + 1 || 99) - (ORDER.indexOf(b.name) + 1 || 99);

const useRoles = () => {
  const [roles, setRoles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await getRoles({ take: 100 });
      setRoles(r.data.map(toRoleRow).sort(byOrder));
      setError(null);
    } catch (e) {
      setError(e);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  return { roles, loading, error, reload: load };
};

export default useRoles;
