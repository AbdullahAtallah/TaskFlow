import { Box, Typography } from "@mui/material";
import ProgressBar from "../../atoms/ProgressBar/ProgressBar";

const TeamBarRow = ({ name, count, max }) => {
  return (
    <Box
      sx={{
        display: "grid",
        gridTemplateColumns: "145px 1fr 40px",
        alignItems: "center",
        gap: "0px",
      }}
    >
      <Typography
        sx={{ fontSize: "14px", lineHeight: "20px", color: "#111827" }}
      >
        {name}
      </Typography>
      <ProgressBar value={count} max={max} />
      <Typography
        sx={{
          fontSize: "14px",
          color: "#111827",
          textAlign: "right",
          fontWeight: "500px",
        }}
      >
        {count}
      </Typography>
    </Box>
  );
};

export default TeamBarRow;
