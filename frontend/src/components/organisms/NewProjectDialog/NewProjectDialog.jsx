import { useEffect, useState } from "react";
import { Box, Stack, Typography } from "@mui/material";
import AppDialog from "../AppDialog/AppDialog";
import FieldLabel from "../../atoms/FieldLabel/FieldLabel";
import TextField from "../../molecules/TextField/TextField";
import SegmentedTabs from "../../molecules/SegmentedTabs/SegmentedTabs";
import AppButton from "../../atoms/AppButton/AppButton";

const NewProjectDialog = ({
  open,
  teams,
  defaultTeamName,
  saving,
  onCreate,
  onClose,
}) => {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [teamName, setTeamName] = useState("");
  const [nameError, setNameError] = useState("");

  useEffect(() => {
    if (!open) return;
    setName("");
    setDescription("");
    setNameError("");
    setTeamName(defaultTeamName ?? teams[0]?.name ?? "");
  }, [open, defaultTeamName, teams]);

  const handleCreate = () => {
    if (!name.trim()) {
      setNameError("This field is required");
      return;
    }
    const team = teams.find((t) => t.name === teamName);
    onCreate({
      teamId: team.id,
      name: name.trim(),
      description: description.trim(),
    });
  };

  return (
    <AppDialog
      open={open}
      title="New project"
      onClose={saving ? undefined : onClose}
      actions={
        <>
          <AppButton variant="secondary" onClick={onClose} disabled={saving}>
            Cancel
          </AppButton>
          <AppButton
            onClick={handleCreate}
            disabled={saving || teams.length === 0}
          >
            {saving ? "Creating…" : "Create project"}
          </AppButton>
        </>
      }
    >
      <Stack spacing="20px">
        <Box>
          <FieldLabel>Project name</FieldLabel>
          <TextField
            placeholder="e.g. Website relaunch"
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
            <Typography sx={{ fontSize: "14px", color: "#9ca3af" }}>
              (optional)
            </Typography>
          </Stack>
          <TextField
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
        </Box>

        <Box>
          <FieldLabel>Team</FieldLabel>
          {teams.length === 0 ? (
            <Typography sx={{ fontSize: "14px", color: "#dc2626" }}>
              Create a team first. A project always belongs to a team.
            </Typography>
          ) : (
            <>
              <Box sx={{ overflowX: "auto" }}>
                <SegmentedTabs
                  options={teams.map((t) => t.name)}
                  value={teamName}
                  onChange={setTeamName}
                  ariaLabel="Team"
                />
              </Box>
              <Typography
                sx={{
                  mt: "8px",
                  fontSize: "13px",
                  lineHeight: "20px",
                  color: "#6b7280",
                }}
              >
                The project belongs to this team. Only its members can see it.
              </Typography>
            </>
          )}
        </Box>
      </Stack>
    </AppDialog>
  );
};

export default NewProjectDialog;
