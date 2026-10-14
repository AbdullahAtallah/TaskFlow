import { Box } from "@mui/material";

const tones = {
  neutral: { backgroundColor: "#f5f5f5", color: "#5d5d5d" },
  purple: { backgroundColor: "#fbf5ff", color: "#6300a5" },
  success: { backgroundColor: "#f2fff2", color: "#13790e" },
  info: { backgroundColor: "#eff7ff", color: "#365aff" },
};

const Tag = ({ children, tone = "neutral" }) => {
  return (
    <Box
      component="span"
      sx={{
        padding: "4px 10px",
        lineHeight: "16px",
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
