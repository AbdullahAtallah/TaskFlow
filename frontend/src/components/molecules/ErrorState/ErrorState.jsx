import { Box, Typography } from "@mui/material";
import AppButton from "../../atoms/AppButton/AppButton";

const ErrorState = ({
  title = "Something went wrong",
  message = "This could not be loaded. Please try again.",
  onRetry,
}) => (
  <Box sx={{ py: "80px", textAlign: "center" }}>
    <Typography
      sx={{
        fontSize: "20px",
        lineHeight: "28px",
        fontWeight: 600,
        color: "#0f1d63",
      }}
    >
      {title}
    </Typography>
    <Typography
      sx={{ mt: "8px", fontSize: "15px", lineHeight: "22px", color: "#6b7280" }}
    >
      {message}
    </Typography>
    {onRetry && (
      <Box sx={{ mt: "20px" }}>
        <AppButton variant="secondary" onClick={onRetry}>
          Try again
        </AppButton>
      </Box>
    )}
  </Box>
);

export default ErrorState;
