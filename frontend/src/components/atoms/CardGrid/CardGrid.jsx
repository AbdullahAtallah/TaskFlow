import { Box } from "@mui/material";

const CardGrid = ({ gap = "24px", alignItems = "start", sx, children }) => (
  <Box
    sx={[
      {
        display: "grid",
        gridTemplateColumns: { xs: "1fr", lg: "1fr 1fr" },
        gap,
        alignItems,
      },
      ...(Array.isArray(sx) ? sx : [sx]),
    ]}
  >
    {children}
  </Box>
);

export default CardGrid;
