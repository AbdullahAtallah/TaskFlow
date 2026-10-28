/* oxlint-disable react/only-export-components -- context files export their hook next to the provider */
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import { useNavigate } from "react-router";
import { useSnackbar } from "notistack";

import { createPermissions } from "../auth/permissions";
import { getCurrentUser } from "../api/user";
import { logout } from "../api/auth";
import { getToken, clearToken } from "../auth/tokenStorage";

const CurrentUserContext = createContext(null);

export const CurrentUserProvider = ({ children }) => {
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();

  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(() => Boolean(getToken()));
  const [loadError, setLoadError] = useState(false);

  const reloadUser = useCallback(async () => {
    if (!getToken()) {
      setUser(null);
      setLoading(false);
      return null;
    }

    try {
      const current = await getCurrentUser();
      setUser(current);
      setLoadError(false);
      return current;
    } catch (err) {
      if (err.response?.status === 401) {
        clearToken();
        setUser(null);
      } else {
        setLoadError(true);
      }
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  const retryUser = useCallback(async () => {
    setLoading(true);
    setLoadError(false);
    await reloadUser();
  }, [reloadUser]);

  useEffect(() => {
    reloadUser();
  }, [reloadUser]);

  useEffect(() => {
    const handleForbidden = () => {
      enqueueSnackbar("You don't have permission for this.", {
        variant: "toast",
        tone: "error",
        title: "No permission",
      });
      reloadUser();
    };

    window.addEventListener("api:forbidden", handleForbidden);
    return () => window.removeEventListener("api:forbidden", handleForbidden);
  }, [enqueueSnackbar, reloadUser]);

  const signOut = useCallback(async () => {
    try {
      await logout();
    } catch {}
    clearToken();
    setUser(null);

    navigate("/sign-in", { replace: true });
  }, [navigate]);

  const value = useMemo(
    () => ({
      user,
      loading,
      loadError,
      reloadUser,
      retryUser,
      signOut,
      ...createPermissions(user),
    }),
    [user, loading, loadError, reloadUser, retryUser, signOut],
  );

  return (
    <CurrentUserContext.Provider value={value}>
      {children}
    </CurrentUserContext.Provider>
  );
};

export const useCurrentUser = () => useContext(CurrentUserContext);
