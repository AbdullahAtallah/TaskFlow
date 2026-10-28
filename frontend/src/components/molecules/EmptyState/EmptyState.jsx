import { Box, Typography } from "@mui/material";

const EmptyState = ({ title, text, sx }) => (
  <Box
    sx={[
      { py: "48px", px: "16px", textAlign: "center" },
      ...(Array.isArray(sx) ? sx : [sx]),
    ]}
  >
    <Typography sx={{ fontSize: "15px", fontWeight: 500, color: "#111827" }}>
      {title}
    </Typography>
    {text && (
      <Typography sx={{ mt: "4px", fontSize: "14px", color: "#6b7280" }}>
        {text}
      </Typography>
    )}
  </Box>
);

export default EmptyState;
