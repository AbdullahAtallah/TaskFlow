import { Box } from "@mui/material";

const NavCount = ({ children }) => {
  return (
    <Box
      component="span"
      sx={{
        minWidth: "17px",
        height: "17px",
        display: "inline-flex",
        alignItems: "center",
        justifyContent: "center",
        fontSize: "12px",
        fontWeight: 500,
        color: "#717171",
        backgroundColor: "#ffffff",
        border: "1px solid #e5e7eb",
        borderRadius: "4px",
        boxSizing: "border-box",
      }}
    >
      {children}
    </Box>
  );
};

export default NavCount;
