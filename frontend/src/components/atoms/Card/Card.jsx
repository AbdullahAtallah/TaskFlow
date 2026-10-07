import { Paper } from "@mui/material";

const Card = ({ children, sx }) => {
  return (
    <Paper
      elevation={0}
      sx={{
        border: "1px solid #e5e7eb",
        borderRadius: "8px",
        overflow: "hidden",
        backgroundColor: "#ffffff",
        ...sx,
      }}
    >
      {children}
    </Paper>
  );
};

export default Card;
