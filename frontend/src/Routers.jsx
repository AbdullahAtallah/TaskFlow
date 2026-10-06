import { Navigate, Route, Routes } from "react-router";
import SignInPage from "./pages/SignInPage/SignInPage";
import CreateAccountPage from "./pages/CreateAccountPage/CreateAccountPage";
import AppLayout from "./components/templates/AppLayout/AppLayout";
import DashboardPage from "./pages/DashboardPage/DashboardPage";
import TeamPage from "./pages/TeamsPage/TeamsPage";

const Routers = () => {
  return (
    <Routes>
      <Route path="/sign-in" element={<SignInPage />} />
      <Route path="/create-account" element={<CreateAccountPage />} />

      <Route element={<AppLayout />}>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/teams" element={<TeamPage />} />
      </Route>
    </Routes>
  );
};

export default Routers;
