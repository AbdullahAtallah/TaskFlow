import { Box } from "@mui/material";

const tones = {
  neutral: { backgroundColor: "#f5f5f5", color: "#5d5d5d" },
  purple: { backgroundColor: "#fbf5ff", color: "#6300a5" },
};

const Tag = ({ children, tone = "neutral" }) => {
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
        ...(tones[tone] ?? tones.neutral),
      }}
    >
      {children}
    </Box>
  );
};

export default Tag;
