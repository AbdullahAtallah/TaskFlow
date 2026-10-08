import { Box, Typography } from "@mui/material";

const StatTile = ({ label, value, color = "#0f1d63" }) => {
  return (
    <Box sx={{ p: "20px", boxSizing: "border-box" }}>
      <Typography
        sx={{ fontSize: "13px", color: "#6b7280", lineHeight: "18ox" }}
      >
        {label}
      </Typography>
      <Typography
        sx={{
          fontSize: "32px",
          fontWeight: 700,
          color,
          lineHeight: "40px",
          letterSpacing: "-0.02em",
          mt: "4px",
        }}
      >
        {value}
      </Typography>
    </Box>
  );
};

export default StatTile;
