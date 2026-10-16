import { Button, Stack, Typography } from "@mui/material";

const UnsavedChangesBar = ({ saving, onDiscard, onSave }) => (
  <Stack
    direction="row"
    spacing="12px"
    sx={{
      alignItems: "center",
      px: "16px",
      py: "12px",
      border: "1px solid #e5e7eb",
      borderRadius: "8px",
      backgroundColor: "#f9fafb",
    }}
  >
    <Typography
      sx={{ flex: 1, fontSize: "15px", lineHeight: "22px", color: "#313131" }}
    >
      You have unsaved changes.
    </Typography>
    <Button
      onClick={onDiscard}
      disabled={saving}
      sx={{
        textTransform: "none",
        height: "38px",
        px: "14px",
        borderRadius: "6px",
        border: "1px solid #e5e7eb",
        backgroundColor: "#ffffff",
        color: "#111827",
        fontWeight: 500,
        fontSize: 14,
      }}
    >
      Discard
    </Button>
    <Button
      variant="contained"
      onClick={onSave}
      disabled={saving}
      sx={{
        textTransform: "none",
        height: "38px",
        px: "14px",
        borderRadius: "6px",
        backgroundColor: "#365aff",
        boxShadow: 0,
        fontWeight: 500,
        fontSize: 14,
        "&:hover": { boxShadow: 0 },
      }}
    >
      {saving ? "Saving…" : "Save"}
    </Button>
  </Stack>
);

export default UnsavedChangesBar;
