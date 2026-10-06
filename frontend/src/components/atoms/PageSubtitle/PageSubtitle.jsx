import { Typography } from "@mui/material";

const PageSubtitle = ({ children }) => {
  return (
    <Typography
      sx={{ mt: "4px", fontSize: "14px", color: "#6b7280", lineHeight: "20px" }}
    >
      {children}
    </Typography>
  );
};

export default PageSubtitle;
