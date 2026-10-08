import { Box, Typography } from "@mui/material";
import CardTitle from "../../atoms/CardTitle/CardTitle";

const CardHeader = ({ title, meta }) => {
  return (
    <Box
      sx={{
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",

        px: "20px",
        height: "60px",
        flexShrink: 0,
        borderBottom: "1px solid #e5e7eb",
      }}
    >
      <CardTitle>{title}</CardTitle>
      {meta && (
        <Typography sx={{ fontSize: "13px", color: "#6b7280" }}>
          {meta}
        </Typography>
      )}
    </Box>
  );
};

export default CardHeader;
