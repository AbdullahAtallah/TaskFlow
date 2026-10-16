import { Box, Checkbox, Stack, Typography } from "@mui/material";
import UserAvatar from "../../atoms/UserAvatar/UserAvatar";

const SelectableUserRow = ({ user, checked, onToggle }) => (
  <Stack
    component="label"
    direction="row"
    spacing="14px"
    sx={{
      alignItems: "center",
      minHeight: "64px",
      px: "18px",
      cursor: "pointer",
      borderBottom: "1px solid #efefef",
      "&:last-of-type": { borderBottom: 0 },
    }}
  >
    <Checkbox
      checked={checked}
      onChange={onToggle}
      disableRipple
      sx={{
        p: 0,
        color: "#d1d5db",
        "& .MuiSvgIcon-root": { fontSize: "22px" },
        "&.Mui-checked": { color: "#365aff" },
      }}
    />
    <UserAvatar name={user.displayName} />
    <Box sx={{ minWidth: 0 }}>
      <Typography
        sx={{ fontSize: "15px", lineHeight: "21px", color: "#111827" }}
      >
        {user.displayName}
      </Typography>
      <Typography
        sx={{ fontSize: "14px", lineHeight: "20px", color: "#6b7280" }}
      >
        {user.userName}
      </Typography>
    </Box>
  </Stack>
);

export default SelectableUserRow;
