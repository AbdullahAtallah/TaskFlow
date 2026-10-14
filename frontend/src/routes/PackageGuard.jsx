import { Navigate, Outlet } from "react-router";
import { useCurrentUser } from "../context/CurrentUserContext";

const PackageGuard = ({ pkg }) => {
  const { hasPackage } = useCurrentUser();
  return hasPackage(pkg) ? <Outlet /> : <Navigate to="/dashboard" replace />;
};

export default PackageGuard;