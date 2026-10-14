import { Box, Typography } from "@mui/material";

const FormField = ({ label, htmlFor, action, hint, error, children }) => {
  return (
    <Box>
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          mb: 1,
        }}
      >
        <Typography
          component="label"
          htmlFor={htmlFor}
          sx={{ fontSize: "14px", color: "#5a5c65", fontWeight: 500 }}
        >
          {label}
        </Typography>
        {action}
      </Box>

      {children}

      {error ? (
        <Typography sx={{ color: "#991b1b", fontSize: "13px", mt: "6px" }}>
          {error}
        </Typography>
      ) : (
        hint && (
          <Typography sx={{ color: "#9a8f8f", fontSize: "14px", mt: 1 }}>
            {hint}
          </Typography>
        )
      )}
    </Box>
  );
};

export default FormField;
