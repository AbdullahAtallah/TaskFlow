import { Navigate, Outlet, useLocation } from "react-router";
import { getToken } from "../auth/tokenStorage";
import { useCurrentUser } from "../context/CurrentUserContext";
import FullPageLoader from "../components/atoms/FullPageLoader/FullPageLoader";
import ErrorState from "../components/molecules/ErrorState/ErrorState";

const AuthGuard = () => {
  const location = useLocation();
  const { user, loading, loadError, retryUser } = useCurrentUser();

  if (!getToken()) {
    return <Navigate to="/sign-in" replace state={{ from: location }} />;
  }

  if (loading) return <FullPageLoader />;

  if (!user) {
    return loadError ? (
      <ErrorState
        title="Can't reach the server"
        message="TaskFlow could not connect. Check that the API is running, then try again."
        onRetry={retryUser}
      />
    ) : (
      <Navigate to="/sign-in" replace state={{ from: location }} />
    );
  }

  return <Outlet />;
};

export default AuthGuard;
