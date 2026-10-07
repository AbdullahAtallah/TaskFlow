import { Typography } from "@mui/material";

const ClaimText = ({ children }) => {
  return (
    <Typography
      component="code"
      sx={{
        display: "block",
        p: "3px 0",
        fontFamily: "ui-monospace, Consolas, 'Courier New', monospace",
        fontSize: "13px",
        lineHeight: "18px",
        color: "#0f1d63",
        backgroundColor: "#ffffff",
      }}
    >
      {children}
    </Typography>
  );
};

export default ClaimText;
