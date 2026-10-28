import { Stack } from "@mui/material";

const Row = ({ gap = "12px", sx, children }) => (
  <Stack
    direction="row"
    sx={[
      { alignItems: "center", flexWrap: "wrap", gap },
      ...(Array.isArray(sx) ? sx : [sx]),
    ]}
  >
    {children}
  </Stack>
);

export default Row;
