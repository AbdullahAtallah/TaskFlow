import { Checkbox, FormControlLabel, Link, Typography } from "@mui/material";
const AcceptTerms = ({ checked, onChange }) => {
  return (
    <FormControlLabel
      control={
        <Checkbox
          checked={checked}
          onChange={onChange}
          sx={{
            "& .MuiSvgIcon-root": {
              fontSize: 20,
            },
          }}
        />
      }
      label={
        <Typography sx={{ color: "#434343", fontSize: "14px" }}>
          I agree to the <Link underline="hover">terms of use</Link> and{" "}
          <Link underline="hover">privacy statement</Link>
        </Typography>
      }
    />
  );
};

export default AcceptTerms;
