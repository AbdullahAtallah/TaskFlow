import { useEffect, useState } from "react";
import { Box, Stack, Typography } from "@mui/material";
import Card from "../../atoms/Card/Card";
import FieldLabel from "../../atoms/FieldLabel/FieldLabel";
import TextField from "../../molecules/TextField/TextField";
import UnsavedChangesBar from "../../molecules/UnsavedChangesBar/UnsavedChangesBar";
import { formatDate } from "../../../utils";

const Stat = ({ label, value }) => (
  <Box sx={{ flex: 1 }}>
    <Typography sx={{ fontSize: "14px", lineHeight: "20px", color: "#6b7280" }}>
      {label}
    </Typography>
    <Typography
      sx={{
        fontSize: "15px",
        lineHeight: "22px",
        color: "#111827",
        fontWeight: 500,
      }}
    >
      {value}
    </Typography>
  </Box>
);

const TeamGeneralCard = ({ team, canManage, saving, onSave }) => {
  const [name, setName] = useState(team.name);
  const [description, setDescription] = useState(team.description ?? "");
  const [nameError, setNameError] = useState("");

  const reset = () => {
    setName(team.name);
    setDescription(team.description ?? "");
    setNameError("");
  };

  useEffect(reset, [team]);

  const changed =
    name !== team.name || description !== (team.description ?? "");

  const handleSave = () => {
    if (!name.trim()) {
      setNameError("Enter a team name.");
      return;
    }
    onSave({ name: name.trim(), description: description.trim() });
  };

  return (
    <>
      <Card sx={{ p: "20px 24px" }}>
        <Stack spacing="20px">
          <Box>
            <FieldLabel>Team name</FieldLabel>
            <TextField
              value={name}
              disabled={!canManage}
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
              disabled={!canManage}
              onChange={(e) => setDescription(e.target.value)}
            />
          </Box>

          <Stack
            direction="row"
            sx={{ pt: "16px", borderTop: "1px solid #e5e7eb" }}
          >
            <Stat label="Members" value={team.members} />
            <Stat label="Projects" value={team.projects} />
            <Stat label="Created" value={formatDate(team.created)} />
          </Stack>
        </Stack>
      </Card>

      {canManage && changed && (
        <UnsavedChangesBar
          saving={saving}
          onDiscard={reset}
          onSave={handleSave}
        />
      )}
    </>
  );
};

export default TeamGeneralCard;
