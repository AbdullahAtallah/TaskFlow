import { Typography } from "@mui/material";

const PageSubtitle = ({ children }) => {
  return (
    <Typography sx={{ mt: "4px", fontSize: "15px", color: "#6b7280" }}>
      {children}
    </Typography>
  );
};

export default PageSubtitle;
