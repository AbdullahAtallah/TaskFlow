import { Box, Typography } from "@mui/material";
import { Link as RouterLink } from "react-router";
import TextLink from "../../atoms/TextLink/TextLink";

const NotFoundState = ({ title = "Not found", message, backTo, backLabel }) => (
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
    {message && (
      <Typography
        sx={{
          mt: "8px",
          fontSize: "15px",
          lineHeight: "22px",
          color: "#6b7280",
        }}
      >
        {message}
      </Typography>
    )}
    {backTo && (
      <Box sx={{ mt: "20px" }}>
        <TextLink component={RouterLink} to={backTo}>
          {backLabel}
        </TextLink>
      </Box>
    )}
  </Box>
);

export default NotFoundState;
