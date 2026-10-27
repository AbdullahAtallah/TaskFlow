import { Box, IconButton } from "@mui/material";
import { Menu } from "lucide-react";
import { useLocation, Link } from "react-router";
import AppBreadcrumbs from "../../molecules/AppBreadcrumbs/AppBreadcrumbs";
import UserInfo from "../../molecules/UserInfo/UserInfo";
import { useCurrentUser } from "../../../context/CurrentUserContext";

const crumbsByPath = {
  "/dashboard": ["Dashboard"],
  "/teams": ["Teams"],
  "/projects": ["Projects"],
  "/profile": ["Profile"],
  "/management/users": ["Management", "Users"],
  "/management/roles": ["Management", "Roles"],
};

const Topbar = ({ onMenuClick }) => {
  const { pathname } = useLocation();
  const { user } = useCurrentUser();
  const isSmall = Boolean(onMenuClick);

  const roleNames = user?.roles?.map((role) => role.name).join(", ") ?? "";

  return (
    <Box
      component="header"
      sx={{
        height: "100%",
        display: "flex",
        alignItems: "center",
        gap: "12px",
        px: isSmall ? "16px" : "32px",
        borderBottom: "1px solid #e5e7eb",
        backgroundColor: "#ffffff",
        boxSizing: "border-box",
      }}
    >
      {isSmall && (
        <IconButton
          onClick={onMenuClick}
          aria-label="Open menu"
          disableRipple
          sx={{
            ml: "-8px",
            p: "8px",
            color: "#434343",
            borderRadius: "6px",
          }}
        >
          <Menu size={22} strokeWidth={1.8} />
        </IconButton>
      )}
      <Box
        sx={{
          flex: 1,
          minWidth: 0,
          overflow: "hidden",
          whiteSpace: "nowrap",
          textOverflow: "ellipsis",
          "& .MuiBreadcrumbs-ol": { flexWrap: "nowrap" },
          "& .MuiBreadcrumbs-li": { whiteSpace: "nowrap", flexShrink: 0 },
          "& .MuiBreadcrumbs-separator": { flexShrink: 0 },
          "& .MuiBreadcrumbs-li:last-of-type": {
            flexShrink: 1,
            minWidth: 0,
            overflow: "hidden",
            textOverflow: "ellipsis",
          },
        }}
      >
        <AppBreadcrumbs items={crumbsByPath[pathname] ?? []} />
      </Box>
      <Link to="/profile" style={{ textDecoration: "none", flexShrink: 0 }}>
        <UserInfo
          name={user?.displayName ?? ""}
          email={user?.userName ?? ""}
          role={roleNames}
          compact={isSmall}
        />
      </Link>
    </Box>
  );
};

export default Topbar;
