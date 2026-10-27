import { useState, useEffect } from "react";
import { Box, Drawer, useMediaQuery } from "@mui/material";
import { Outlet, useLocation } from "react-router";
import Sidebar from "../../organisms/Sidebar/Sidebar";
import Topbar from "../../organisms/Topbar/Topbar";
import { BreadcrumbProvider } from "../../../context/BreadcrumbContext";

const SIDEBAR_WIDTH = "265px";

const AppLayout = () => {
  const isSmall = useMediaQuery("(max-width:899.95px)");
  const { pathname } = useLocation();
  const [menuOpen, setMenuOpen] = useState(false);

  useEffect(() => {
    setMenuOpen(false);
  }, [pathname]);

  useEffect(() => {
    if (!isSmall) setMenuOpen(false);
  }, [isSmall]);

  return (
    <BreadcrumbProvider>
      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: isSmall ? "1fr" : `${SIDEBAR_WIDTH} 1fr`,
          gridTemplateRows: "64px 1fr",
          gridTemplateAreas: isSmall
            ? `"topbar" "main"`
            : `"sidebar topbar" "sidebar main"`,
          height: "100vh",
          overflow: "hidden",
        }}
      >
        {!isSmall && (
          <Box sx={{ gridArea: "sidebar", minHeight: 0 }}>
            <Sidebar />
          </Box>
        )}

        <Box sx={{ gridArea: "topbar", minWidth: 0 }}>
          <Topbar onMenuClick={isSmall ? () => setMenuOpen(true) : undefined} />
        </Box>

        <Box
          component="main"
          sx={{
            gridArea: "main",
            overflowY: "auto",
            minHeight: 0,
            minWidth: 0,
            backgroundColor: "#ffffff",
          }}
        >
          <Outlet />
        </Box>
      </Box>
      {isSmall && (
        <Drawer
          open={menuOpen}
          onClose={() => setMenuOpen(false)}
          transitionDuration={0}
          slotProps={{ paper: { sx: { width: SIDEBAR_WIDTH, border: 0 } } }}
        >
          <Sidebar />
        </Drawer>
      )}
    </BreadcrumbProvider>
  );
};

export default AppLayout;
