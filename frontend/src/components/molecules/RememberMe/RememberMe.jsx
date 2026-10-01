import { Checkbox, FormControlLabel } from "@mui/material";
const RememberMe = ({ checked, onChange }) => {
  return (
    <FormControlLabel
      control={<Checkbox checked={checked} onChange={onChange} size="small" />}
      label="Keep me signed in"
      sx={{ color: "#434343" }}
    />
  );
};

export default RememberMe;
