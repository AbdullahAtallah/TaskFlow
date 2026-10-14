import { Navigate, Outlet, useLocation } from "react-router";
import { getToken } from "../auth/tokenStorage";
import { useCurrentUser } from "../context/CurrentUserContext";
import FullPageLoader from "../components/atoms/FullPageLoader/FullPageLoader";

const AuthGuard = () => {
  const location = useLocation();
  const { user, loading } = useCurrentUser();

  if (!getToken()) {
    return <Navigate to="/sign-in" replace state={{ from: location }} />;
  }

  if (loading || !user) return <FullPageLoader />;

  return <Outlet />;
};

export default AuthGuard;
