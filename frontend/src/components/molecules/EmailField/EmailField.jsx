import { TextField } from "@mui/material";
const EmailField = ({ value, onChange }) => {
  return (
    <TextField
      fullWidth
      placeholder="name@example.com"
      type="email"
      value={value}
      onChange={onChange}
      sx={{
        boxSizing: "border-box",
        fontSize: "14px",
        fontWeight: 400,
        borderRadius: "6px",
      }}
    />
  );
};

export default EmailField;
