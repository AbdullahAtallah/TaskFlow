import { useCallback, useEffect, useState } from "react";
import { getTaskBoard } from "../api/tasks";

const useTaskBoard = (projectId, enabled) => {
  const [columns, setColumns] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const load = useCallback(async () => {
    if (!enabled) return [];
    setLoading(true);
    setError(null);
    try {
      const board = await getTaskBoard(projectId);
      setColumns(board.columns);
      return board.columns;
    } catch (err) {
      setError(err);
      return [];
    } finally {
      setLoading(false);
    }
  }, [projectId, enabled]);

  useEffect(() => {
    load();
  }, [load]);

  return { columns, loading, error, reload: load };
};

export default useTaskBoard;
