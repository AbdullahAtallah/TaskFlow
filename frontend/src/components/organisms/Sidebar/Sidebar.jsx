import { Box, Stack } from "@mui/material";
import { House, Users, Folder, User, Shield, LogOut } from "lucide-react";
import AppLogo from "../../atoms/AppLogo/AppLogo";
import NavSection from "../../molecules/NavSection/NavSection";
import NavItem from "../../molecules/NavItem/NavItem";
import { useCurrentUser } from "../../../context/CurrentUserContext";
import { Package } from "../../../constants/permissions";
import useMyTaskCount from "../../../hooks/useMyTaskCount";

const sections = [
  {
    label: "Workspace",
    items: [
      { to: "/dashboard", label: "Dashboard", icon: House },
      { to: "/teams", label: "Teams", icon: Users, pkg: Package.Teams },
      {
        to: "/projects",
        label: "Projects",
        icon: Folder,
        pkg: Package.Projects,
      },
    ],
  },
  {
    label: "Management",
    pkg: Package.Management,
    items: [
      { to: "/management/users", label: "Users", icon: User },
      { to: "/management/roles", label: "Roles", icon: Shield },
    ],
  },
];

const Sidebar = () => {
  const { hasPackage, signOut } = useCurrentUser();
  const myTaskCount = useMyTaskCount();

  const allowed = (entry) => !entry.pkg || hasPackage(entry.pkg);

  const visibleSections = sections
    .filter(allowed)
    .map((section) => ({
      ...section,
      items: section.items
        .filter(allowed)
        .map((item) =>
          item.to === "/dashboard" && myTaskCount > 0
            ? { ...item, count: myTaskCount }
            : item,
        ),
    }))
    .filter((section) => section.items.length > 0);

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
        {visibleSections.map((section) => (
          <NavSection
            key={section.label}
            label={section.label}
            items={section.items}
          />
        ))}
      </Box>

      <Stack spacing="3px" sx={{ px: "13px", pb: "20px" }}>
        <NavItem to="/profile" label="Profile" icon={User} />
        <NavItem label="Sign out" icon={LogOut} onClick={signOut} />
      </Stack>
    </Box>
  );
};

export default Sidebar;
