import { Button } from "@mui/material";

const base = {
  textTransform: "none",
  height: "38px",
  px: "14px",
  borderRadius: "6px",
  fontWeight: 500,
  fontSize: 14,
  lineHeight: "20px",
  whiteSpace: "nowrap",
  flexShrink: 0,
  boxShadow: 0,
  "&.Mui-disabled": { opacity: 0.6 },
};

const variants = {
  primary: {
    muiVariant: "contained",
    sx: {
      backgroundColor: "#365aff",
      color: "#ffffff",
      "&.Mui-disabled": {
        backgroundColor: "#365aff",
        color: "#ffffff",
        opacity: 0.6,
      },
    },
  },
  secondary: {
    muiVariant: "outlined",
    sx: {
      backgroundColor: "#ffffff",
      color: "#111827",
      border: "1px solid #e5e7eb",
      "&.Mui-disabled": {
        color: "#111827",
        border: "1px solid #e5e7eb",
        opacity: 0.6,
      },
    },
  },
  danger: {
    muiVariant: "contained",
    sx: {
      backgroundColor: "#dc2626",
      color: "#ffffff",
      "&.Mui-disabled": {
        backgroundColor: "#dc2626",
        color: "#ffffff",
        opacity: 0.6,
      },
    },
  },
};

const AppButton = ({ variant = "primary", sx, children, ...props }) => {
  const v = variants[variant] ?? variants.primary;

  return (
    <Button
      variant={v.muiVariant}
      disableRipple
      disableElevation
      sx={[base, v.sx, ...(Array.isArray(sx) ? sx : [sx])]}
      {...props}
    >
      {children}
    </Button>
  );
};

export default AppButton;
