import { useEffect, useState } from "react";
import { useLocation } from "react-router";
import { getMyTasks } from "../api/dashboard";

const useMyTaskCount = (enabled = true) => {
  const { pathname } = useLocation();
  const [count, setCount] = useState(null);

  useEffect(() => {
    if (!enabled) {
      setCount(null);
      return;
    }
    getMyTasks({ take: 1 })
      .then((r) => setCount(r.total))
      .catch(() => setCount(null));
  }, [pathname, enabled]);

  return count;
};

export default useMyTaskCount;
