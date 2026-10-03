import { Box, Typography } from "@mui/material";

const AuthFooter = () => {
  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        gap: 2,
      }}
    >
      <Typography
        sx={{
          color: "#6b7280",
          fontSize: "12px",
        }}
      >
        © 2026 TaskFlow
      </Typography>

      <Typography
        sx={{
          color: "#6b7280",
          fontSize: "12px",
        }}
      >
        Secured by Auth0
      </Typography>
    </Box>
  );
};

export default AuthFooter;
