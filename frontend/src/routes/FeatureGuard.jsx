import { Navigate, Outlet } from "react-router";
import { useCurrentUser } from "../context/CurrentUserContext";

const FeatureGuard = ({ feature }) => {
  const { hasFeature } = useCurrentUser();
  return hasFeature(feature) ? <Outlet /> : <Navigate to="/dashboard" replace />;
};

export default FeatureGuard;