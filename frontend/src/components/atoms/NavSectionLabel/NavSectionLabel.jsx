import { Typography } from "@mui/material";

const NavSectionLabel = ({ children }) => {
  return (
    <Typography
      sx={{
        p: "14px 10px 4px",
        fontSize: "10px",
        fontWeight: 500,
        letterSpacing: "0.08em",
        textTransform: "uppercase",
        color: "#767676",
      }}
    >
      {children}
    </Typography>
  );
};

export default NavSectionLabel;
