import { Box } from "@mui/material";

const ClampText = ({ children, lines = 2 }) => {
  return (
    <Box
      component="span"
      sx={{
        display: "-webkit-box",
        WebkitLineClamp: lines,
        WebkitBoxOrient: "vertical",
        overflow: "hidden",
      }}
    >
      {children}
    </Box>
  );
};

export default ClampText;
