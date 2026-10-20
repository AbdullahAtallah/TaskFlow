import { Box, Dialog, IconButton, Stack, Typography } from "@mui/material";
import { X } from "lucide-react";

const AppDialog = ({
  open,
  title,
  onClose,
  actions,
  children,
  width = "600px",
}) => (
  <Dialog
    open={open}
    onClose={onClose}
    slotProps={{
      paper: {
        sx: {
          width,
          maxWidth: "calc(100% - 32px)",
          borderRadius: "12px",
          boxShadow: "0 20px 40px rgba(15, 29, 99, 0.15)",
          overflowX: "hidden",
          overflowY: "auto",
        },
      },
      backdrop: { sx: { backgroundColor: "rgba(15, 29, 99, 0.3)" } },
    }}
  >
    <Stack
      direction="row"
      sx={{
        alignItems: "center",
        justifyContent: "space-between",
        px: "24px",
        py: "20px",
        borderBottom: "1px solid #e5e7eb",
      }}
    >
      <Typography
        component="h2"
        sx={{
          fontSize: "18px",
          lineHeight: "28px",
          fontWeight: 600,
          color: "#0f1d63",
        }}
      >
        {title}
      </Typography>
      <IconButton
        onClick={onClose}
        disableRipple
        sx={{ p: "4px", color: "#6b7280" }}
        aria-label="Close"
      >
        <X size={18} />
      </IconButton>
    </Stack>

    <Box
      sx={{
        p: "24px",
        fontSize: "14px",
        lineHeight: "22px",
        color: "#434343",
        textWrap: "pretty",
      }}
    >
      {children}
    </Box>

    {actions && (
      <Stack
        direction="row"
        spacing="12px"
        sx={{
          justifyContent: "flex-end",
          px: "26px",
          py: "18px",
          backgroundColor: "#f9f9fb",
          borderTop: "1px solid #e5e7eb",
        }}
      >
        {actions}
      </Stack>
    )}
  </Dialog>
);

export default AppDialog;
