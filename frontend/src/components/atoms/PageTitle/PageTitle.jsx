import { Typography } from "@mui/material";

const PageTitle = ({ children }) => {
  return (
    <Typography
      component="h1"
      sx={{ fontSize: "24px", fontWeight: 700, color: "#0f1d63", m: 0 }}
    >
      {children}
    </Typography>
  );
};

export default PageTitle;
