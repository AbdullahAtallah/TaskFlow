import { Box, Typography } from "@mui/material";

const PageToolbar = ({ children, count }) => {
  return (
    <Box
      sx={{
        display: "flex",
        flexWrap: "wrap",
        alignItems: "center",
        justifyContent: "space-between",
        gap: "16px",
        mb: "24px",
      }}
    >
      <Box
        sx={{
          minWidth: 0,
          maxWidth: "100%",
          flex: { xs: "1 1 100%", sm: "0 1 auto" },
        }}
      >
        {children}
      </Box>

      <Typography
        sx={{ fontSize: "14px", lineHeight: "20px", color: "#6b7280" }}
      >
        {count}
      </Typography>
    </Box>
  );
};

export default PageToolbar;
