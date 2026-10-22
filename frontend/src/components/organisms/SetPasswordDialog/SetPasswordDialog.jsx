import { useEffect, useState } from "react";
import { Box, Button, Stack } from "@mui/material";
import AppDialog from "../AppDialog/AppDialog";
import FieldLabel from "../../atoms/FieldLabel/FieldLabel";
import TextField from "../../molecules/TextField/TextField";

const secondaryBtn = {
  textTransform: "none",
  height: "38px",
  px: "14px",
  borderRadius: "6px",
  border: "1px solid #e5e7eb",
  backgroundColor: "#ffffff",
  color: "#111827",
  fontWeight: 500,
  fontSize: 14,
};
const primaryBtn = {
  textTransform: "none",
  height: "38px",
  px: "14px",
  borderRadius: "6px",
  backgroundColor: "#365aff",
  boxShadow: 0,
  fontWeight: 500,
  fontSize: 14,
};

const SetPasswordDialog = ({ open, userName, saving, onSave, onClose }) => {
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (!open) return;
    setPassword("");
    setConfirm("");
    setErrors({});
  }, [open]);

  const handleSave = () => {
    const next = {};
    if (!password) next.password = "This field is required";
    else if (password.length < 8) next.password = "Use at least 8 characters";
    if (!confirm) next.confirm = "This field is required";
    else if (password && confirm !== password)
      next.confirm = "The passwords do not match";

    setErrors(next);
    if (Object.values(next).some(Boolean)) return;

    onSave(password);
  };

  return (
    <AppDialog
      open={open}
      title={`Set password for ${userName ?? ""}`}
      onClose={saving ? undefined : onClose}
      actions={
        <>
          <Button onClick={onClose} disabled={saving} sx={secondaryBtn}>
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleSave}
            disabled={saving}
            sx={primaryBtn}
          >
            {saving ? "Saving…" : "Set password"}
          </Button>
        </>
      }
    >
      <Stack spacing="20px">
        <Box>
          <FieldLabel>New password</FieldLabel>
          <TextField
            type="password"
            placeholder="At least 8 characters"
            value={password}
            autoFocus
            autoComplete="new-password"
            onChange={(e) => {
              setPassword(e.target.value);
              setErrors((prev) => ({ ...prev, password: "" }));
            }}
            error={Boolean(errors.password)}
            helperText={errors.password}
          />
        </Box>

        <Box>
          <FieldLabel>Confirm password</FieldLabel>
          <TextField
            type="password"
            value={confirm}
            autoComplete="new-password"
            onChange={(e) => {
              setConfirm(e.target.value);
              setErrors((prev) => ({ ...prev, confirm: "" }));
            }}
            error={Boolean(errors.confirm)}
            helperText={errors.confirm}
          />
        </Box>
      </Stack>
    </AppDialog>
  );
};

export default SetPasswordDialog;
