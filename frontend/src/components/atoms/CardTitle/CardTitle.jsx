import { Typography } from "@mui/material";

const CardTitle = ({ children }) => {
  return (
    <Typography
      component="h2"
      sx={{
        fontSize: "16px",
        fontWeight: 600,
        lineHeight: "24px",
        color: "#111827",
        m: 0,
      }}
    >
      {children}
    </Typography>
  );
};

export default CardTitle;
