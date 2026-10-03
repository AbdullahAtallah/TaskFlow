import { Checkbox, FormControlLabel, Typography } from "@mui/material";
const RememberMe = ({ checked, onChange }) => {
  return (
    <FormControlLabel
      control={
        <Checkbox
          checked={checked}
          onChange={onChange}
          sx={{
            "& .MuiSvgIcon-root": {
              fontSize: 20,
              color: "#365aff",
            },
          }}
        />
      }
      label={
        <Typography sx={{ color: "#434343", fontSize: "14px" }}>
          Keep me signed in
        </Typography>
      }
    />
  );
};

export default RememberMe;
