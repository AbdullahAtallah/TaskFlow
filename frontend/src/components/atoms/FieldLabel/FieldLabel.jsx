import { Typography } from "@mui/material";

const FieldLabel = ({ children }) => {
  return (
    <Typography sx={{ fontSize: "14px", color: "#6b7280", mb: "8px" }}>
      {children}
    </Typography>
  );
};

export default FieldLabel;
