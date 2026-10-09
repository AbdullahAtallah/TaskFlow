import { IconButton } from "@mui/material";
import VisibilityOutlined from "@mui/icons-material/VisibilityOutlined";
import VisibilityOffOutlined from "@mui/icons-material/VisibilityOffOutlined";

const iconSx = { "&.MuiSvgIcon-root": { fontSize: "18px" } };

const PasswordToggle = ({ visible, onToggle }) => {
  return (
    <IconButton
      onClick={onToggle}
      edge="end"
      aria-label={visible ? "Hide password" : "Show password"}
    >
      {visible ? (
        <VisibilityOffOutlined sx={iconSx} />
      ) : (
        <VisibilityOutlined sx={iconSx} />
      )}
    </IconButton>
  );
};

export default PasswordToggle;
