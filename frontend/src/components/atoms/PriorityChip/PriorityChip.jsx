import { Box } from "@mui/material";

const styles = {
  High: { backgroundColor: "#feebec", color: "#c70e0e" },
  Medium: { backgroundColor: "#fff5eb", color: "#cf580f" },
  Low: { backgroundColor: "#f5f5f5", color: "#5d5d5d" },
};

const PriorityChip = ({ priority }) => {
  return (
    <Box
      component="span"
      sx={{
        height: "16px",
        p: "4px 10px",
        display: "inline-flex",
        alignItems: "center",
        borderRadius: "999px",
        fontSize: "12px",
        fontWeight: 500,
        whiteSpace: "nowrap",
        ...(styles[priority] ?? styles.Low),
      }}
    >
      {priority}
    </Box>
  );
};

export default PriorityChip;
