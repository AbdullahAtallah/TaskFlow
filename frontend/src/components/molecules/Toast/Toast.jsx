import { forwardRef } from "react";
import { SnackbarContent, useSnackbar } from "notistack";
import { Box, Button, IconButton, Typography } from "@mui/material";
import { X } from "lucide-react";

const toneColors = {
  success: "#16a34a",
  error: "#dc2626",
  info: "#365aff",
};

const Toast = forwardRef(
  ({ id, message, title, tone = "success", actionLabel, onAction }, ref) => {
    const { closeSnackbar } = useSnackbar();

    const handleAction = () => {
      onAction?.();
      closeSnackbar(id);
    };

    return (
      <SnackbarContent ref={ref} role="status">
        <Box
          sx={{
            width: "320px",
            height: "156",
            maxWidth: "calc(100vw - 32px)",
            backgroundColor: "#ffffff",
            borderRadius: "8px",
            borderTop: `6px solid ${toneColors[tone] ?? toneColors.success}`,
            boxShadow:
              "0 10px 25px -5px rgba(0,0,0,0.12), 0 4px 10px -4px rgba(0,0,0,0.08)",
            p: "20px",
            boxSizing: "border-box",
          }}
        >
          <Box
            sx={{
              display: "flex",
              alignItems: "flex-start",
              justifyContent: "space-between",
              gap: "12px",
            }}
          >
            <Box>
              <Typography
                sx={{
                  fontSize: "14px",
                  fontWeight: 500,
                  lineHeight: "20px",
                  color: "#111827",
                }}
              >
                {title}
              </Typography>
              <Typography
                sx={{
                  fontSize: "14px",
                  lineHeight: "20px",
                  color: "#6b7280",
                  mt: "2px",
                }}
              >
                {message}
              </Typography>
            </Box>

            <IconButton
              size="small"
              aria-label="Close"
              onClick={() => closeSnackbar(id)}
              sx={{ color: "#111827", mt: "-4px", mr: "-6px" }}
            >
              <X size={16} />
            </IconButton>
          </Box>

          {actionLabel && (
            <Box
              sx={{ display: "flex", justifyContent: "flex-end", mt: "20px" }}
            >
              <Button
                variant="outlined"
                sx={{
                  textTransform: "none",
                  backgroundColor: "#ffffff",
                  color: "#111827",
                  border: "1px solid #e5e7eb",
                  fontWeight: 500,
                  fontSize: 14,
                  p: "0 12px",
                  height: "38px",
                  borderRadius: "6px",
                  boxShadow: 0,
                }}
                onClick={handleAction}
              >
                {actionLabel}
              </Button>
            </Box>
          )}
        </Box>
      </SnackbarContent>
    );
  },
);

Toast.displayName = "Toast";

export default Toast;
