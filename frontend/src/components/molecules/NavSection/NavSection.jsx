import { Box, Stack } from "@mui/material";
import NavSectionLabel from "../../atoms/NavSectionLabel/NavSectionLabel";
import NavItem from "../NavItem/NavItem";

const NavSection = ({ label, items }) => {
  return (
    <Box>
      <NavSectionLabel>{label}</NavSectionLabel>
      <Stack spacing="2px">
        {items.map((item) => (
          <NavItem key={item.to} {...item} />
        ))}
      </Stack>
    </Box>
  );
};

export default NavSection;
