import { Switch as MuiSwitch } from "@mui/material";

const Switch = ({ checked, onChange, disabled, ariaLabel }) => {
  return (
    <MuiSwitch
      checked={checked}
      onChange={(e) => onChange?.(e.target.checked)}
      disabled={disabled}
      disableRipple
      slotProps={{ input: { "aria-label": ariaLabel } }}
      sx={{
        width: 44,
        height: 24,
        p: 0,
        flexShrink: 0,
        "& .MuiSwitch-switchBase": {
          p: "3px",
          transitionDuration: "200ms",
          "&:hover": { backgroundColor: "transparent" },
          "&.Mui-checked": {
            transform: "translateX(20px)",
            color: "#ffffff",
            "& + .MuiSwitch-track": { backgroundColor: "#365aff", opacity: 1 },
          },
          "&.Mui-disabled + .MuiSwitch-track": { opacity: 0.45 },
          "&.Mui-disabled .MuiSwitch-thumb": { color: "#ffffff" },
        },
        "& .MuiSwitch-thumb": {
          width: 18,
          height: 18,
          boxShadow: "none",
          color: "#ffffff",
        },
        "& .MuiSwitch-track": {
          borderRadius: "12px",
          backgroundColor: "#d1d5db",
          opacity: 1,
          transition: "none",
        },
      }}
    />
  );
};

export default Switch;
