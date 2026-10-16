import { useState } from "react";
import { Button, Stack, Typography } from "@mui/material";
import { useSnackbar } from "notistack";
import TeamMembersTable from "../TeamMembersTable/TeamMembersTable";
import useTeamMembers from "../../../hooks/useTeamMembers";
import { removeTeamMember } from "../../../api/teams";
import afterWrite from "../../../utils/afterWrite";

const TeamMembersTab = ({ teamId, canManage, onChanged }) => {
  const { members, loading, error, reload } = useTeamMembers(teamId);
  const { enqueueSnackbar } = useSnackbar();
  const [removingId, setRemovingId] = useState(null);

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
    } catch {
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
      <Typography sx={{ fontSize: 14, color: "#dc2626" }}>
        Could not load the members.
      </Typography>
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
          <Button
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
            Add members
          </Button>
        )}
      </Stack>

      <TeamMembersTable
        members={members}
        canManage={canManage}
        removingId={removingId}
        onRemove={handleRemove}
      />
    </Stack>
  );
};

export default TeamMembersTab;
