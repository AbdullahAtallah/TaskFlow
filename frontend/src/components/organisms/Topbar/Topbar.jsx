import { Box } from "@mui/material";
import { useLocation } from "react-router";
import AppBreadcrumbs from "../../molecules/AppBreadcrumbs/AppBreadcrumbs";
import UserInfo from "../../molecules/UserInfo/UserInfo";

const crumbsByPath = {
  "/dashboard": ["Dashboard"],
  "/teams": ["Teams"],
  "/projects": ["Projects"],
  "/profile": ["Profile"],
  "/management/users": ["Management", "Users"],
  "/management/roles": ["Management", "Roles"],
};

const user = { name: "Sara Jansen", email: "sara@example.com", role: "Admin" };

const Topbar = () => {
  const { pathname } = useLocation();

  return (
    <Box
      component="header"
      sx={{
        height: "100%",
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        px: "32px",
        borderBottom: "1px solid #e5e7eb",
        backgroundColor: "#ffffff",
        boxSizing: "border-box",
      }}
    >
      <AppBreadcrumbs items={crumbsByPath[pathname] ?? []} />
      <UserInfo name={user.name} email={user.email} role={user.role} />
    </Box>
  );
};

export default Topbar;
