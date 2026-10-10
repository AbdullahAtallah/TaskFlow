import { Stack } from "@mui/material";
import TextLink from "../../atoms/TextLink/TextLink";

const RowActions = ({ onEdit, onDelete }) => {
  return (
    <Stack direction="row" sx={{ gap: "16px", alignItems: "center" }}>
      <TextLink onClick={onEdit}>Edit</TextLink>
      <TextLink onClick={onDelete}>Delete</TextLink>
    </Stack>
  );
};

export default RowActions;
