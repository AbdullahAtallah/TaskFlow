import { Box, CircularProgress } from "@mui/material";

const LoadingState = ({ py = "48px" }) => (
  <Box sx={{ py, display: "flex", justifyContent: "center" }}>
    <CircularProgress size={24} thickness={5} sx={{ color: "#365aff" }} />
  </Box>
);

export default LoadingState;
