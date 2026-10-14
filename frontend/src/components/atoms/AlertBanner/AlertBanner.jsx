import { Box } from "@mui/material";

const AlertBanner = ({ children }) => {
  if (!children) return null;

  return (
    <Box
      role="alert"
      sx={{
        mb: 2,
        px: "12px",
        py: "10px",
        borderRadius: "8px",
        border: "1px solid #e91b2a",
        backgroundColor: "#fef2f2",
        color: "#991b1b",
        fontSize: "14px",
        lineHeight: "20px",
      }}
    >
      {children}
    </Box>
  );
};

export default AlertBanner;
