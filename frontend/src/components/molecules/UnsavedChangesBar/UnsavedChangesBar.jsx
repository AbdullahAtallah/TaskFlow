import { Stack, Typography } from "@mui/material";
import AppButton from "../../atoms/AppButton/AppButton";

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
    <AppButton variant="secondary" onClick={onDiscard} disabled={saving}>
      Discard
    </AppButton>
    <AppButton onClick={onSave} disabled={saving}>
      {saving ? "Saving…" : "Save"}
    </AppButton>
  </Stack>
);

export default UnsavedChangesBar;
