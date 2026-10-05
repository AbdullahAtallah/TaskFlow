import { Box, Stack, Typography } from "@mui/material";
import RoleChip from "../../atoms/RoleChip/RoleChip";
import UserAvatar from "../../atoms/UserAvatar/UserAvatar";

const UserInfo = ({ name, email, role }) => {
  return (
    <Stack
      direction="row"
      sx={{ alignItems: "center", gap: "13px", cursor: "pointer" }}
    >
      <RoleChip>{role}</RoleChip>
      <UserAvatar name={name} online />
      <Box>
        <Typography
          sx={{
            fontSize: "14px",
            fontWeight: 500,
            color: "#1f2937",
            lineHeight: 1.3,
          }}
        >
          {name}
        </Typography>
        <Typography
          sx={{ fontSize: "12px", color: "#6b7280", lineHeight: 1.3 }}
        >
          {email}
        </Typography>
      </Box>
    </Stack>
  );
};

export default UserInfo;
