import { useState } from "react";
import { InputAdornment, TextField as MuiTextField } from "@mui/material";
import PasswordToggle from "../../atoms/PasswordToggle/PasswordToggle";

const TextField = ({
  value,
  onChange,
  placeholder,
  type = "text",
  fullWidth = true,
  sx,
  ...props
}) => {
  const [showPassword, setShowPassword] = useState(false);
  const isPassword = type === "password";

  return (
    <MuiTextField
      fullWidth={fullWidth}
      placeholder={placeholder}
      type={isPassword && showPassword ? "text" : type}
      value={value}
      onChange={onChange}
      {...props}
      slotProps={{
        input: isPassword
          ? {
              endAdornment: (
                <InputAdornment position="end">
                  <PasswordToggle
                    visible={showPassword}
                    onToggle={() => setShowPassword((show) => !show)}
                  />
                </InputAdornment>
              ),
            }
          : undefined,
      }}
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

export default TextField;
