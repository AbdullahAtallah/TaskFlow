import { useEffect, useState } from "react";
import { Box, Stack, Typography } from "@mui/material";
import AppDialog from "../AppDialog/AppDialog";
import WizardSteps from "../../molecules/WizardSteps/WizardSteps";
import SelectableUserRow from "../../molecules/SelectableUserRow/SelectableUserRow";
import FieldLabel from "../../atoms/FieldLabel/FieldLabel";
import TextField from "../../molecules/TextField/TextField";
import AppButton from "../../atoms/AppButton/AppButton";
import { getUsers } from "../../../api/user";

const STEPS = ["Details", "Members", "Summary"];

const SummaryRow = ({ label, value }) => (
  <Stack direction="row">
    <Typography
      sx={{
        width: "148px",
        flexShrink: 0,
        fontSize: "14px",
        lineHeight: "20px",
        color: "#6b7280",
      }}
    >
      {label}
    </Typography>
    <Typography sx={{ fontSize: "14px", lineHeight: "20px", color: "#111827" }}>
      {value}
    </Typography>
  </Stack>
);

const NewTeamDialog = ({ open, currentUserId, saving, onCreate, onClose }) => {
  const [step, setStep] = useState(0);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [nameError, setNameError] = useState("");
  const [users, setUsers] = useState([]);
  const [memberIds, setMemberIds] = useState([]);

  useEffect(() => {
    if (!open) return;
    setStep(0);
    setName("");
    setDescription("");
    setNameError("");
    setMemberIds(currentUserId ? [currentUserId] : []);
    getUsers({ take: 100, sort: { field: "displayName", dir: "asc" } }).then(
      (res) => setUsers(res.data),
    );
  }, [open, currentUserId]);

  const toggle = (id) =>
    setMemberIds((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id],
    );

  const goNext = () => {
    if (step === 0 && !name.trim()) {
      setNameError("This field is required");
      return;
    }
    setStep((s) => s + 1);
  };

  const memberNames = users
    .filter((u) => memberIds.includes(u.id))
    .map((u) => u.displayName)
    .join(", ");

  const actions =
    step === 0 ? (
      <>
        <AppButton variant="secondary" onClick={onClose}>
          Cancel
        </AppButton>
        <AppButton onClick={goNext}>Next</AppButton>
      </>
    ) : step === 1 ? (
      <>
        <AppButton variant="secondary" onClick={() => setStep(0)}>
          Back
        </AppButton>
        <AppButton onClick={goNext}>Next</AppButton>
      </>
    ) : (
      <>
        <AppButton
          variant="srcondary"
          onClick={() => setStep(1)}
          disabled={saving}
        >
          Back
        </AppButton>
        <AppButton
          disabled={saving}
          onClick={() =>
            onCreate({
              name: name.trim(),
              description: description.trim(),
              memberIds,
            })
          }
        >
          {saving ? "Creating…" : "Create team"}
        </AppButton>
      </>
    );

  return (
    <AppDialog
      open={open}
      title="New team"
      onClose={saving ? undefined : onClose}
      actions={actions}
    >
      <Stack spacing="24px">
        <WizardSteps steps={STEPS} current={step} />

        {step === 0 && (
          <Stack spacing="20px">
            <Box>
              <FieldLabel>Team name</FieldLabel>
              <TextField
                placeholder="e.g. Facilities"
                value={name}
                autoFocus
                onChange={(e) => {
                  setName(e.target.value);
                  setNameError("");
                }}
                error={Boolean(nameError)}
                helperText={nameError}
              />
            </Box>
            <Box>
              <Stack direction="row" sx={{ justifyContent: "space-between" }}>
                <FieldLabel>Description</FieldLabel>
                <Typography sx={{ fontSize: "14px", color: "#9a8f8f" }}>
                  (optional)
                </Typography>
              </Stack>
              <TextField
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
            </Box>
          </Stack>
        )}

        {step === 1 && (
          <Box
            sx={{
              border: "1px solid #e5e7eb",
              borderRadius: "8px",
              maxHeight: "420px",
              overflowY: "auto",
            }}
          >
            {users.map((user) => (
              <SelectableUserRow
                key={user.id}
                user={user}
                checked={memberIds.includes(user.id)}
                onToggle={() => toggle(user.id)}
              />
            ))}
          </Box>
        )}

        {step === 2 && (
          <Stack spacing="14px">
            <SummaryRow label="Name" value={name.trim()} />
            <SummaryRow label="Description" value={description.trim() || "—"} />
            <SummaryRow label="Members" value={memberNames || "—"} />
          </Stack>
        )}
      </Stack>
    </AppDialog>
  );
};

export default NewTeamDialog;
