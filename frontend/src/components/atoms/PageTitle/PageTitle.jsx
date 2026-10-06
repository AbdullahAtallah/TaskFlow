import { Typography } from "@mui/material";

const PageTitle = ({ children }) => {
  return (
    <Typography
      component="h1"
      sx={{
        fontSize: "24px",
        fontWeight: 600,
        color: "#0f1d63",
        m: 0,
        lineHeight: "32px",
      }}
    >
      {children}
    </Typography>
  );
};

export default PageTitle;
