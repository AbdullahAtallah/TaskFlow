import { Button, Typography } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import TextField from "../../components/molecules/TextField/TextField";
import TeamsTable from "../../components/organisms/TeamsTable/TeamsTable";
import PageToolbar from "../../components/molecules/PageToolbar/PageToolbar ";
import AppDialog from "../../components/organisms/AppDialog/AppDialog";
import NewTeamDialog from "../../components/organisms/NewTeamDialog/NewTeamDialog";
import useTeams from "../../hooks/useTeams";
import useDebouncedValue from "../../hooks/useDebouncedValue";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { Claim } from "../../constants/permissions";
import { deleteTeam, createTeam } from "../../api/teams";
import { useSnackbar } from "notistack";
import { useState } from "react";
import { useNavigate } from "react-router";
import afterWrite from "../../utils/afterWrite";

const TeamsPage = () => {
  const navigate = useNavigate();

  const [search, setSearch] = useState("");
  const query = useDebouncedValue(search);
  const { teams, total, loading, error, reload } = useTeams(query);
  const { user, hasClaim } = useCurrentUser();
  const canManage = hasClaim(Claim.TeamsManage);
  const { enqueueSnackbar } = useSnackbar();
  const [blockedTeam, setBlockedTeam] = useState(null);
  const [newTeamOpen, setNewTeamOpen] = useState(false);
  const [creating, setCreating] = useState(false);

  const handleCreate = async ({ name, description, memberIds }) => {
    setCreating(true);
    try {
      await createTeam({ name, description, memberIds });
      await afterWrite(reload);
      enqueueSnackbar(`${name} was created.`, {
        variant: "toast",
        tone: "success",
        title: "Team created",
      });
      setNewTeamOpen(false);
    } catch {
      enqueueSnackbar("The team could not be created.", {
        variant: "toast",
        tone: "error",
        title: "Something went wrong",
      });
    } finally {
      setCreating(false);
    }
  };

  const handleDelete = async (team) => {
    if (team.projects > 0) {
      setBlockedTeam(team);
      return;
    }

    try {
      await deleteTeam(team.id);
      await afterWrite(reload);
      enqueueSnackbar(`${team.name} was deleted.`, {
        variant: "toast",
        tone: "success",
        title: "Team deleted",
      });
    } catch {
      enqueueSnackbar(`${team.name} could not be deleted.`, {
        variant: "toast",
        tone: "error",
        title: "Something went wrong",
      });
    }
  };

  const canViewAll = hasClaim(Claim.TeamsViewAll);

  const emptyTitle = search
    ? "No teams found"
    : canViewAll
      ? "No teams yet"
      : "You're not in any team yet";
  const emptyText = search
    ? `Nothing matches "${search}".`
    : canViewAll
      ? "Create the first team to get started."
      : "Ask an admin to add you to a team.";

  const countLabel = loading ? "Loading…" : `${total} teams`;
  return (
    <>
      <PageHeader
        title="Teams"
        subtitle="All teams in the organisation"
        actions={
          canManage && (
            <Button
              variant="contained"
              onClick={() => setNewTeamOpen(true)}
              sx={{
                textTransform: "none",
                backgroundColor: "#365aff",
                fontWeight: 500,
                fontSize: 14,
                p: "0 12px",
                height: "38px",
                borderRadius: "6px",
                boxShadow: 0,
              }}
            >
              New team
            </Button>
          )
        }
      />
      <PageBody>
        <PageToolbar count={countLabel}>
          <TextField
            type="search"
            placeholder="Search teams"
            fullWidth={false}
            sx={{ width: "320px" }}
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
        </PageToolbar>

        {error ? (
          <Typography sx={{ fontSize: 14, color: "#dc2626" }}>
            Could not load teams. Please try again.
          </Typography>
        ) : (
          <TeamsTable
            teams={teams}
            canManage={canManage}
            onEdit={(team) => navigate(`/teams/${team.id}`)}
            onDelete={handleDelete}
            emptyTitle={emptyTitle}
            emptyText={emptyText}
            loading={loading}
          />
        )}
      </PageBody>
      <AppDialog
        open={Boolean(blockedTeam)}
        title="Team cannot be deleted"
        onClose={() => setBlockedTeam(null)}
        actions={
          <>
            <Button
              onClick={() => setBlockedTeam(null)}
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
              Cancel
            </Button>
            <Button
              variant="contained"
              onClick={() => setBlockedTeam(null)}
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
              Close
            </Button>
          </>
        }
      >
        {blockedTeam?.name} still owns {blockedTeam?.projects} active{" "}
        {blockedTeam?.projects === 1 ? "project" : "projects"}. Move or delete
        these projects first.
      </AppDialog>
      <NewTeamDialog
        open={newTeamOpen}
        currentUserId={user.id}
        saving={creating}
        onCreate={handleCreate}
        onClose={() => setNewTeamOpen(false)}
      />
    </>
  );
};

export default TeamsPage;
