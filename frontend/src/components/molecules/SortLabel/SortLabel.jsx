import { Box } from "@mui/material";
import SortIcon from "../../atoms/SortIcon/SortIcon";

const SortLabel = ({ children, active = false }) => {
  return (
    <Box sx={{ display: "inline-flex", alignItems: "center", gap: "6px" }}>
      {children}
      <SortIcon active={active} />
    </Box>
  );
};

export default SortLabel;
