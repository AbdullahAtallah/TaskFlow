import { Box, Typography } from "@mui/material";

const PageToolbar = ({ children, count }) => {
  return (
    <Box
      sx={{
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        gap: "16px",
        mb: "24px",
      }}
    >
      {children}
      <Typography
        sx={{ fontSize: "14px", lineHeight: "20px", color: "#6b7280" }}
      >
        {count}
      </Typography>
    </Box>
  );
};

export default PageToolbar;
