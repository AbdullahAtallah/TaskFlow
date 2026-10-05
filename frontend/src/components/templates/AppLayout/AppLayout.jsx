import { Box } from "@mui/material";
import { Outlet } from "react-router";
import Sidebar from "../../organisms/Sidebar/Sidebar";
import Topbar from "../../organisms/Topbar/Topbar";

const AppLayout = () => {
  return (
    <Box
      sx={{
        display: "grid",
        gridTemplateColumns: "265px 1fr",
        gridTemplateRows: "64px 1fr",
        gridTemplateAreas: `"sidebar topbar" "sidebar main"`,
        height: "100vh",
        overflow: "hidden",
      }}
    >
      <Box sx={{ gridArea: "sidebar", minHeight: 0 }}>
        <Sidebar />
      </Box>

      <Box sx={{ gridArea: "topbar" }}>
        <Topbar />
      </Box>

      <Box
        component="main"
        sx={{
          gridArea: "main",
          overflowY: "auto",
          minHeight: 0,
          backgroundColor: "#ffffff",
        }}
      >
        <Outlet />
      </Box>
    </Box>
  );
};

export default AppLayout;
