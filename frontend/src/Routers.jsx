import { Navigate, Route, Routes } from "react-router";
import { Package } from "./constants/permissions";

import GuestGuard from "./routes/GuestGuard";
import AuthGuard from "./routes/AuthGuard";
import PackageGuard from "./routes/PackageGuard";

import SignInPage from "./pages/SignInPage/SignInPage";
import CreateAccountPage from "./pages/CreateAccountPage/CreateAccountPage";
import AppLayout from "./components/templates/AppLayout/AppLayout";
import DashboardPage from "./pages/DashboardPage/DashboardPage";
import TeamPage from "./pages/TeamsPage/TeamsPage";
import TeamDetailsPage from "./pages/TeamDetailsPage/TeamDetailsPage";
import ProjectsPage from "./pages/ProjectsPage/ProjectsPage";
import UsersPage from "./pages/UsersPage/UsersPage";
import RolesPage from "./pages/RolesPage/RolesPage";
import ProfilePage from "./pages/ProfilePage/ProfilePage";
import ProjectDetailsPage from "./pages/ProjectDetailsPage/ProjectDetailsPage";

import NotFoundPage from "./pages/NotFoundPage/NotFoundPage";

const Routers = () => {
  return (
    <Routes>
      <Route element={<GuestGuard />}>
        <Route path="/sign-in" element={<SignInPage />} />
        <Route path="/create-account" element={<CreateAccountPage />} />
      </Route>

      <Route element={<AuthGuard />}>
        <Route element={<AppLayout />}>
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/profile" element={<ProfilePage />} />

          <Route element={<PackageGuard pkg={Package.Teams} />}>
            <Route path="/teams" element={<TeamPage />} />
            <Route path="/teams/:id" element={<TeamDetailsPage />} />
          </Route>

          <Route element={<PackageGuard pkg={Package.Projects} />}>
            <Route path="/projects" element={<ProjectsPage />} />
            <Route path="/projects/:id" element={<ProjectDetailsPage />} />
          </Route>

          <Route element={<PackageGuard pkg={Package.Management} />}>
            <Route path="/management/users" element={<UsersPage />} />
            <Route path="/management/roles" element={<RolesPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
};

export default Routers;
