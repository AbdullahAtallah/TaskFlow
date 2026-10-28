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

import { getCurrentUser } from "../api/user";
import { logout } from "../api/auth";
import { getToken, clearToken } from "../auth/tokenStorage";

const CurrentUserContext = createContext(null);

export const CurrentUserProvider = ({ children }) => {
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();

  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(() => Boolean(getToken()));

  const reloadUser = useCallback(async () => {
    if (!getToken()) {
      setUser(null);
      setLoading(false);
      return null;
    }

    try {
      const current = await getCurrentUser();
      setUser(current);
      return current;
    } catch {
      clearToken();
      setUser(null);
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

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

  const value = useMemo(() => {
    const packages = user?.packages ?? [];
    const features = user?.features ?? [];
    const claims = user?.claims ?? [];

    return {
      user,
      loading,
      reloadUser,
      signOut,
      hasPackage: (pkg) => packages.includes(pkg),
      hasFeature: (feature) => features.includes(feature),
      hasClaim: (claim) => claims.includes(claim),
      hasAnyClaim: (...list) => list.some((claim) => claims.includes(claim)),
    };
  }, [user, loading, reloadUser, signOut]);

  return (
    <CurrentUserContext.Provider value={value}>
      {children}
    </CurrentUserContext.Provider>
  );
};

export const useCurrentUser = () => useContext(CurrentUserContext);
