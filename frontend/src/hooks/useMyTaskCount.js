import { useEffect, useState } from "react";
import { useLocation } from "react-router";
import { getMyTasks } from "../api/dashboard";

const useMyTaskCount = () => {
  const { pathname } = useLocation();
  const [count, setCount] = useState(null);

  useEffect(() => {
    getMyTasks({ take: 1 })
      .then((r) => setCount(r.total))
      .catch(() => setCount(null));
  }, [pathname]);

  return count;
};

export default useMyTaskCount;
