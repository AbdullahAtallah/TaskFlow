import { Box, Paper } from "@mui/material";
import AppLogo from "../../atoms/AppLogo/AppLogo";
import AuthFooter from "../../molecules/AuthFooter/AuthFooter";

const AuthLayout = ({ children }) => {
  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "flex",
        flexDirection: "column",
        alignItems: "center",

        backgroundColor: "#F8F8FA",
      }}
    >
      <Box sx={{ mb: 3 }}>
        <AppLogo />
      </Box>

      <Paper
        sx={{
          width: "440px",
          maxWidth: "100%",
          border: "1px solid #e5e7eb",
          boxSizing: "border-box",
          borderRadius: "12px",
          boxShadow: "0 4px 6px -1px #00000014",
          p: "40px",
          display: "flex",
          flexDirection: "column",
          gap: "24px",
        }}
      >
        {children}
      </Paper>

      <Box sx={{ mt: 3 }}>
        <AuthFooter />
      </Box>
    </Box>
  );
};

export default AuthLayout;
