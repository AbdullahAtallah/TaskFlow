import { Avatar, Badge } from "@mui/material";

const getInitials = (name = "") =>
  name
    .split(" ")
    .filter(Boolean)
    .map((part) => part[0])
    .filter((ch) => ch === ch.toUpperCase())
    .slice(0, 2)
    .join("");

const UserAvatar = ({ name, size = 32, online = false, sx }) => {
  const avatar = (
    <Avatar
      sx={{
        width: size,
        height: size,
        fontSize: 13,
        fontWeight: 600,
        bgcolor: "#e9f7ef",
        color: "#1b7a5e",
        border: ".5px solid #d2d2d2",
        ...sx,
      }}
    >
      {getInitials(name)}
    </Avatar>
  );

  if (!online) return avatar;

  return (
    <Badge
      overlap="circular"
      variant="dot"
      anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
      sx={{
        "& .MuiBadge-badge": {
          width: "7px",
          height: "7px",
          minWidth: "7px",
          borderRadius: "50%",
          backgroundColor: "#1ea618",
          border: "2px solid #ffffff",
          boxSizing: "content-box",
        },
      }}
    >
      {avatar}
    </Badge>
  );
};

export default UserAvatar;
