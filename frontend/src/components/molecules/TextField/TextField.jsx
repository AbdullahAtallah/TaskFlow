import { TextField } from "@mui/material";
const EmailField = ({ value, onChange }) => {
  return (
    <TextField
      fullWidth
      placeholder="e.g. Laura Bakker"
      type="text"
      value={value}
      onChange={onChange}
      sx={{
        "& .MuiOutlinedInput-input": { py: 1.25, fontSize: 14 },
        "& .MuiOutlinedInput-notchedOutline": {
          borderWidth: 0.5,
          borderColor: "#E4E7EC",
          borderRadius: 1.5,
        },
        "& .MuiOutlinedInput-root:hover .MuiOutlinedInput-notchedOutline": {
          borderColor: "#E4E7EC",
        },
        "& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline":
          {
            border: 0,
            boxShadow: "0 0 0 4px #365AFF2E",
          },
      }}
    />
  );
};

export default EmailField;
