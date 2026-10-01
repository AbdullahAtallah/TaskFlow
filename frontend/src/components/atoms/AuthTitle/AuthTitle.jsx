import { Typography } from "@mui/material";
const AuthTitle = ({ children }) => {
  return (
    <Typography
      variant="h4"
      component="h1"
      sx={{ fontWeight: 700, mb: 1, color: "#0f1d63", fontSize: "24px" }}
    >
      {children}
    </Typography>
  );
};

export default AuthTitle;
