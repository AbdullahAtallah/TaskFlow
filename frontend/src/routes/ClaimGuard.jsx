import { Navigate, Outlet } from "react-router";
import { useCurrentUser } from "../context/CurrentUserContext";

const ClaimGuard = ({ claim }) => {
  const { hasClaim } = useCurrentUser();
  return hasClaim(claim) ? <Outlet /> : <Navigate to="/dashboard" replace />;
};

export default ClaimGuard;
