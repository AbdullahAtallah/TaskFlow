import { useState, useMemo } from "react";
import { Stack, Typography } from "@mui/material";
import { useSnackbar } from "notistack";
import TeamMembersTable from "../TeamMembersTable/TeamMembersTable";
import AddMembersDialog from "../AddMembersDialog/AddMembersDialog";
import AppButton from "../../atoms/AppButton/AppButton";
import ErrorState from "../../molecules/ErrorState/ErrorState";

import useTeamMembers from "../../../hooks/useTeamMembers";
import { addTeamMembers, removeTeamMember } from "../../../api/teams";
import afterWrite from "../../../utils/afterWrite";
import { isHandled } from "../../../utils/apiErrors";

const TeamMembersTab = ({ teamId, teamName, canManage, onChanged }) => {
  const { members, loading, error, reload } = useTeamMembers(teamId);
  const { enqueueSnackbar } = useSnackbar();
  const [removingId, setRemovingId] = useState(null);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [adding, setAdding] = useState(false);

  const existingIds = useMemo(() => members.map((m) => m.userId), [members]);

  const handleAdd = async (userIds) => {
    setAdding(true);
    try {
      await addTeamMembers(teamId, userIds);
      await afterWrite(async () => {
        await reload();
        onChanged?.();
      });
      enqueueSnackbar(
        `${userIds.length} ${userIds.length === 1 ? "member was" : "members were"} added to ${teamName}.`,
        { variant: "toast", tone: "success", title: "Members added" },
      );
      setDialogOpen(false);
    } catch (err) {
      if (isHandled(err)) {
        setDialogOpen(false);
        return;
      }
      enqueueSnackbar("The members could not be added.", {
        variant: "toast",
        tone: "error",
        title: "Something went wrong",
      });
    } finally {
      setAdding(false);
    }
  };

  const handleRemove = async (member) => {
    setRemovingId(member.userId);
    try {
      await removeTeamMember(teamId, member.userId);
      await afterWrite(async () => {
        await reload();
        onChanged?.();
      });
      enqueueSnackbar(`${member.displayName} was removed from the team.`, {
        variant: "toast",
        tone: "success",
        title: "Member removed",
      });
    } catch (err) {
      if (isHandled(err)) return;
      enqueueSnackbar(`${member.displayName} could not be removed.`, {
        variant: "toast",
        tone: "error",
        title: "Something went wrong",
      });
    } finally {
      setRemovingId(null);
    }
  };

  if (error) {
    return (
      <ErrorState message="The members could not be loaded." onRetry={reload} />
    );
  }

  return (
    <Stack spacing="24px">
      <Stack
        direction="row"
        sx={{ alignItems: "center", justifyContent: "space-between" }}
      >
        <Typography
          sx={{ fontSize: "15px", lineHeight: "22px", color: "#6b7280" }}
        >
          {loading
            ? "Loading…"
            : `${members.length} ${members.length === 1 ? "member" : "members"}`}
        </Typography>

        {canManage && (
          <AppButton variant="secondary" onClick={() => setDialogOpen(true)}>
            Add members
          </AppButton>
        )}
      </Stack>

      <TeamMembersTable
        members={members}
        loading={loading}
        canManage={canManage}
        removingId={removingId}
        onRemove={handleRemove}
      />
      <AddMembersDialog
        open={dialogOpen}
        teamName={teamName}
        existingIds={existingIds}
        saving={adding}
        onAdd={handleAdd}
        onClose={() => setDialogOpen(false)}
      />
    </Stack>
  );
};

export default TeamMembersTab;
