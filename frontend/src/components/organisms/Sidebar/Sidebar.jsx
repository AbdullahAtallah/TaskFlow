import { Box, Stack } from "@mui/material";
import { House, Users, Folder, User, Shield, LogOut } from "lucide-react";
import AppLogo from "../../atoms/AppLogo/AppLogo";
import NavSection from "../../molecules/NavSection/NavSection";
import NavItem from "../../molecules/NavItem/NavItem";

const sections = [
  {
    label: "Workspace",
    items: [
      { to: "/dashboard", label: "Dashboard", icon: House, count: 6 },
      { to: "/teams", label: "Teams", icon: Users },
      { to: "/projects", label: "Projects", icon: Folder },
    ],
  },
  {
    label: "Management",
    items: [
      { to: "/management/users", label: "Users", icon: User },
      { to: "/management/roles", label: "Roles", icon: Shield },
    ],
  },
];

const Sidebar = () => {
  return (
    <Box
      component="aside"
      sx={{
        height: "100%",
        display: "flex",
        flexDirection: "column",
        backgroundColor: "#f8f9fb",
        borderRight: "1px solid #e5e7eb",
        boxSizing: "border-box",
      }}
    >
      <Box
        sx={{
          height: "64px",
          flexShrink: 0,
          display: "flex",
          alignItems: "center",
          px: "22px",
          borderBottom: "1px solid #e5e7eb",
          boxSizing: "border-box",
        }}
      >
        <AppLogo />
      </Box>

      <Box component="nav" sx={{ flex: 1, overflowY: "auto", px: "13px" }}>
        {sections.map((section) => (
          <NavSection
            key={section.label}
            label={section.label}
            items={section.items}
          />
        ))}
      </Box>

      <Stack spacing="3px" sx={{ px: "13px", pb: "20px" }}>
        <NavItem to="/profile" label="Profile" icon={User} />
        <NavItem to="/sign-in" label="Sign out" icon={LogOut} />
      </Stack>
    </Box>
  );
};

export default Sidebar;
