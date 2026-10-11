import { ToggleButton, ToggleButtonGroup } from "@mui/material";

const SegmentedTabs = ({ options, value, onChange, ariaLabel = "Filter" }) => {
  const handleChange = (event, newValue) => {
    if (newValue !== null) onChange(newValue);
  };

  return (
    <ToggleButtonGroup
      exclusive
      value={value}
      onChange={handleChange}
      aria-label={ariaLabel}
      sx={{
        height: "38px",
        flexShrink: 0,
        border: "1px solid #cecece",
        borderRadius: "6px",
        overflow: "hidden",
        backgroundColor: "#f0f0f0",
        boxSizing: "border-box",

        "& .MuiToggleButton-root": {
          border: 0,
          borderRadius: 0,
          px: "14px",
          fontSize: "14px",
          fontWeight: 500,
          lineHeight: "20px",
          textTransform: "none",
          whiteSpace: "nowrap",
          color: "#717171",
          "&:hover": { backgroundColor: "transparent" },
        },

        "& .MuiToggleButton-root.Mui-selected": {
          backgroundColor: "#ffffff",
          color: "#4a4a4a",
        },
      }}
    >
      {options.map((option) => (
        <ToggleButton key={option} value={option} disableRipple>
          {option}
        </ToggleButton>
      ))}
    </ToggleButtonGroup>
  );
};

export default SegmentedTabs;
