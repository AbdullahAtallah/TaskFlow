import { Navigate, Outlet } from "react-router";
import { getToken } from "../auth/tokenStorage";

const GuestGuard = () => {
  return getToken() ? <Navigate to="/dashboard" replace /> : <Outlet />;
};

export default GuestGuard;
