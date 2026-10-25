import { useEffect, useState } from "react";
import { Box, Stack, Typography } from "@mui/material";
import AppDialog from "../AppDialog/AppDialog";
import FieldLabel from "../../atoms/FieldLabel/FieldLabel";
import TextField from "../../molecules/TextField/TextField";
import SegmentedTabs from "../../molecules/SegmentedTabs/SegmentedTabs";
import AppButton from "../../atoms/AppButton/AppButton";

const EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

const NewUserDialog = ({ open, roles, saving, onCreate, onClose }) => {
  const [displayName, setDisplayName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [roleName, setRoleName] = useState("User");
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (!open) return;
    setDisplayName("");
    setEmail("");
    setPassword("");
    setRoleName(roles.find((r) => r.isDefault)?.name ?? roles[0]?.name ?? "");
    setErrors({});
  }, [open, roles]);

  const clearError = (field) => setErrors((prev) => ({ ...prev, [field]: "" }));

  const handleCreate = () => {
    const next = {};
    if (!displayName.trim()) next.displayName = "This field is required";
    if (!email.trim()) next.email = "This field is required";
    else if (!EMAIL.test(email.trim()))
      next.email = "Enter a valid e-mail address";
    if (!password) next.password = "This field is required";
    else if (password.length < 8) next.password = "Use at least 8 characters";

    setErrors(next);
    if (Object.values(next).some(Boolean)) return;

    const role = roles.find((r) => r.name === roleName);
    onCreate({
      displayName: displayName.trim(),
      userName: email.trim(),
      password,
      roleIds: role ? [role.id] : [],
    });
  };

  return (
    <AppDialog
      open={open}
      title="New user"
      onClose={saving ? undefined : onClose}
      actions={
        <>
          <AppButton variant="secondary" onClick={onClose} disabled={saving}>
            Cancel
          </AppButton>
          <AppButton
            onClick={handleCreate}
            disabled={saving || roles.length === 0}
          >
            {saving ? "Creating…" : "Create user"}
          </AppButton>
        </>
      }
    >
      <Stack spacing="20px">
        <Box>
          <FieldLabel>Display name</FieldLabel>
          <TextField
            placeholder="e.g. Laura Bakker"
            value={displayName}
            autoFocus
            onChange={(e) => {
              setDisplayName(e.target.value);
              clearError("displayName");
            }}
            error={Boolean(errors.displayName)}
            helperText={errors.displayName}
          />
        </Box>

        <Box>
          <FieldLabel>E-mail</FieldLabel>
          <TextField
            placeholder="name@example.com"
            value={email}
            onChange={(e) => {
              setEmail(e.target.value);
              clearError("email");
            }}
            error={Boolean(errors.email)}
            helperText={errors.email}
          />
        </Box>

        <Box>
          <FieldLabel>Password</FieldLabel>
          <TextField
            type="password"
            placeholder="At least 8 characters"
            value={password}
            autoComplete="new-password"
            onChange={(e) => {
              setPassword(e.target.value);
              clearError("password");
            }}
            error={Boolean(errors.password)}
            helperText={errors.password}
          />
        </Box>

        <Box>
          <FieldLabel>Role</FieldLabel>
          {roles.length === 0 ? (
            <Typography sx={{ fontSize: "14px", color: "#6b7280" }}>
              Loading roles…
            </Typography>
          ) : (
            <SegmentedTabs
              options={roles.map((r) => r.name)}
              value={roleName}
              onChange={setRoleName}
              ariaLabel="Role"
            />
          )}
        </Box>
      </Stack>
    </AppDialog>
  );
};

export default NewUserDialog;
