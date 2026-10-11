import { Box } from "@mui/material";
import UserAvatar from "../../atoms/UserAvatar/UserAvatar";

const UserNameCell = ({ name }) => {
  return (
    <Box sx={{ display: "flex", alignItems: "center", gap: "8px" }}>
      <UserAvatar name={name} size={24} sx={{ fontSize: 10 }} />
      <span>{name}</span>
    </Box>
  );
};

export default UserNameCell;
