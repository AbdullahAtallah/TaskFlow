import { Avatar, Badge } from "@mui/material";

const palette = [
  { bg: "#ece0f6", fg: "#6300a5" },
  { bg: "#e7f6e0", fg: "#0f1d63" },
  { bg: "#feebec", fg: "#cf580f" },
  { bg: "#e8f8f5", fg: "#1b7a5e" },
];

const getInitials = (name = "") =>
  name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((word) => word[0].toUpperCase())
    .join("");

const pickColor = (name = "") =>
  palette[name.charCodeAt(0) % palette.length] ?? palette[0];

const UserAvatar = ({ name, size = 32, online = false, sx }) => {
  const { bg, fg } = pickColor(name);

  const avatar = (
    <Avatar
      sx={{
        width: size,
        height: size,
        fontSize: 13,
        fontWeight: 600,
        bgcolor: bg,
        color: fg,
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
