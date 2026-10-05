import { Box, ButtonBase } from "@mui/material";
import { NavLink } from "react-router";
import NavCount from "../../atoms/NavCount/NavCount";

const NavItem = ({ to, icon: Icon, label, count, onClick }) => {
  const linkProps = to ? { component: NavLink, to } : { onClick };

  return (
    <ButtonBase
      {...linkProps}
      sx={{
        width: "100%",
        height: "38px",
        justifyContent: "flex-start",
        gap: "12px",
        px: "11px",
        borderRadius: "8px",
        fontFamily: "inherit",
        fontSize: "14px",
        fontWeight: 500,
        color: "#434343",
        textDecoration: "none",
        "& .nav-icon": { color: "#767676", flexShrink: 0 },

        "&.active": {
          backgroundColor: "#eef4ff",
          color: "#2305ca",
          "& .nav-icon": { color: "#2305ca" },
        },
      }}
    >
      <Icon className="nav-icon" size={20} strokeWidth={1.6} />
      <Box component="span" sx={{ flex: 1, textAlign: "left" }}>
        {label}
      </Box>
      {count != null && <NavCount>{count}</NavCount>}
    </ButtonBase>
  );
};

export default NavItem;
