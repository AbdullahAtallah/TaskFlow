import { Box } from "@mui/material";

const RoleChip = ({ children }) => {
  return (
    <Box
      component="span"
      sx={{
        px: "10px",
        display: "inline-flex",
        alignItems: "center",
        borderRadius: "999px",
        fontSize: "12px",
        fontWeight: 500,
        color: "#5d5d5d",
        backgroundColor: "#f5f5f5",
      }}
    >
      {children}
    </Box>
  );
};

export default RoleChip;
