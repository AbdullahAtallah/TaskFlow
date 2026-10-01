import { Typography } from "@mui/material";

const AuthDescription = ({ children }) => {
  return (
    <Typography sx={{ mb: 4, color: "#6b7280", fontSize: "14px" }}>
      {children}
    </Typography>
  );
};

export default AuthDescription;
