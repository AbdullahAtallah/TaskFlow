import { Breadcrumbs, Typography, Link } from "@mui/material";
import { Link as RouterLink, useLocation } from "react-router";
import { ChevronRight } from "lucide-react";
import { useBreadcrumb } from "../../../context/BreadcrumbContext";

const sections = {
  dashboard: "Dashboard",
  teams: "Teams",
  projects: "Projects",
  profile: "Profile",
  users: "Users",
  roles: "Roles",
};

const AppBreadcrumbs = () => {
  const { pathname } = useLocation();
  const { crumbs } = useBreadcrumb();

  const parts = pathname.split("/").filter(Boolean);
  const isManagement = parts[0] === "management";
  const key = isManagement ? parts[1] : parts[0];

  const fallback = sections[key]
    ? isManagement
      ? [{ label: "Management" }, { label: sections[key] }]
      : [{ label: sections[key] }]
    : [];

  const items = crumbs ?? fallback;

  if (items.length === 0) return null;

  return (
    <Breadcrumbs
      separator={<ChevronRight size={13} color="#6b7280" />}
      sx={{ "& .MuiBreadcrumbs-separator": { mx: "10px" } }}
    >
      {items.map((item, i) => {
        const isLast = i === items.length - 1;
        if (!isLast && item.to) {
          return (
            <Link
              key={i}
              component={RouterLink}
              to={item.to}
              underline="none"
              sx={{
                fontSize: "15px",
                lineHeight: "22px",
                color: "#6b7280",
                "&:hover": { color: "#365aff" },
              }}
            >
              {item.label}
            </Link>
          );
        }

        return (
          <Typography
            key={i}
            sx={{
              fontSize: "15px",
              lineHeight: "22px",
              fontWeight: isLast ? 500 : 400,
              color: isLast ? "#1f2937" : "#6b7280",
            }}
          >
            {item.label}
          </Typography>
        );
      })}
    </Breadcrumbs>
  );
};

export default AppBreadcrumbs;
