import { Box, Stack, Typography } from "@mui/material";
import UserAvatar from "../../atoms/UserAvatar/UserAvatar";

const ProfileIdentity = ({ name, email }) => {
  return (
    <Stack direction="row" sx={{ alignItems: "center", gap: "16px" }}>
      <UserAvatar name={name} size={56} sx={{ fontSize: "24px" }} />
      <Box>
        <Typography
          sx={{
            fontSize: "18px",
            fontWeight: 600,
            color: "#0f1d63",
            lineHeight: "26px",
          }}
        >
          {name}
        </Typography>
        <Typography
          sx={{ fontSize: "14px", color: "#6b7280", lineHeight: "20px" }}
        >
          {email}
        </Typography>
      </Box>
    </Stack>
  );
};

export default ProfileIdentity;
