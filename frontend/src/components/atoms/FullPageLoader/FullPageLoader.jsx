import { Box, CircularProgress } from "@mui/material";

const FullPageLoader = () => {
  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        backgroundColor: "#f8f9fb",
      }}
    >
      <CircularProgress size={28} sx={{ color: "#365aff" }} />
    </Box>
  );
};

export default FullPageLoader;