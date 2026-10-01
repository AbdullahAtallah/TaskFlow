import { TextField, IconButton, InputAdornment } from "@mui/material";
import { useState } from "react";
import Visibility from "@mui/icons-material/Visibility";
import VisibilityOff from "@mui/icons-material/VisibilityOff";

const PasswordField = ({ value, onChange }) => {
  const [showPassword, setShowPassword] = useState(false);

  const clickShowPassword = () => setShowPassword((show) => !show);
  return (
    <TextField
      fullWidth
      type={showPassword ? "text" : "password"}
      value={value}
      onChange={onChange}
      slotProps={{
        input: {
          endAdornment: (
            <InputAdornment position="end">
              <IconButton
                onClick={clickShowPassword}
                edge="end"
                aria-label={showPassword ? "Hhow password" : "Show password"}
              >
                {showPassword ? <VisibilityOff /> : <Visibility />}
              </IconButton>
            </InputAdornment>
          ),
        },
      }}
    />
  );
};

export default PasswordField;
