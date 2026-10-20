import { ButtonBase, Typography } from "@mui/material";
import UserAvatar from "../../atoms/UserAvatar/UserAvatar";

const AssigneeChip = ({ name, selected, onClick }) => (
  <ButtonBase
    onClick={onClick}
    disableRipple
    aria-pressed={selected}
    sx={{
      display: "inline-flex",
      alignItems: "center",
      gap: "8px",
      height: "35px",
      pl: "4px",
      pr: "12px",
      pt: "4px",
      pb: "4px",
      borderRadius: "999px",
      border: `1px solid ${selected ? "#365aff" : "#e5e7eb"}`,
      backgroundColor: selected ? "#eff7ff" : "#ffffff",
    }}
  >
    <UserAvatar name={name} size={24} sx={{ fontSize: "10px" }} />
    <Typography
      sx={{
        fontSize: "13px",
        lineHeight: "22px",
        color: selected ? "#2305ca" : "#111827",
      }}
    >
      {name}
    </Typography>
  </ButtonBase>
);

export default AssigneeChip;
