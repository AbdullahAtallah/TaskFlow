import { Stack } from "@mui/material";

const Column = ({ gap = "24px", sx, children }) => (
  <Stack spacing={gap} sx={sx}>
    {children}
  </Stack>
);

export default Column;
