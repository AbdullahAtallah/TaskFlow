import { TextField, IconButton, InputAdornment } from "@mui/material";
import { useState } from "react";
import VisibilityOutlined from "@mui/icons-material/VisibilityOutlined";
import VisibilityOffOutlined from "@mui/icons-material/VisibilityOffOutlined";

const PasswordField = ({ value, onChange }) => {
  const [showPassword, setShowPassword] = useState(false);

  const clickShowPassword = () => setShowPassword((show) => !show);
  return (
    <TextField
      fullWidth
      type={showPassword ? "text" : "password"}
      value={value}
      onChange={onChange}
      sx={{
        "& .MuiOutlinedInput-input": { py: 1.25, fontSize: 14 },
        "& .MuiOutlinedInput-notchedOutline": {
          borderWidth: 0.5,
          borderColor: "#E4E7EC",
        },
        "& .MuiOutlinedInput-root:hover .MuiOutlinedInput-notchedOutline": {
          borderColor: "#E4E7EC",
        },
        "& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline":
          {
            border: 1,
            color: "#365aff",
            boxShadow: "0 0 0 4px #365AFF2E",
          },
      }}
      slotProps={{
        input: {
          endAdornment: (
            <InputAdornment position="end">
              <IconButton
                onClick={clickShowPassword}
                edge="end"
                aria-label={showPassword ? "Hide password" : "Show password"}
              >
                {showPassword ? (
                  <VisibilityOffOutlined
                    sx={{
                      "&.MuiSvgIcon-root": {
                        fontSize: "18px",
                      },
                    }}
                  />
                ) : (
                  <VisibilityOutlined
                    sx={{
                      "&.MuiSvgIcon-root": {
                        fontSize: "18px",
                      },
                    }}
                  />
                )}
              </IconButton>
            </InputAdornment>
          ),
        },
      }}
    />
  );
};

export default PasswordField;
