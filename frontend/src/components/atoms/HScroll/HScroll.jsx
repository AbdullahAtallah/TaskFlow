import { Box } from "@mui/material";

const HScroll = ({ children }) => (
  <Box sx={{ overflowX: "auto", p: "2px", m: "-2px" }}>{children}</Box>
);

export default HScroll;
