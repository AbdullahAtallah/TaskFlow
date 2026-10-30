import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import TextField from "../../components/molecules/TextField/TextField";
import TeamsTable from "../../components/organisms/TeamsTable/TeamsTable";
import PageToolbar from "../../components/molecules/PageToolbar/PageToolbar";
import AppDialog from "../../components/organisms/AppDialog/AppDialog";
import NewTeamDialog from "../../components/organisms/NewTeamDialog/NewTeamDialog";
import AppButton from "../../components/atoms/AppButton/AppButton";
import ErrorState from "../../components/molecules/ErrorState/ErrorState";
import useTeams from "../../hooks/useTeams";
import useDebouncedValue from "../../hooks/useDebouncedValue";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { Claim } from "../../constants/permissions";
import { deleteTeam, createTeam } from "../../api/teams";
import { useSnackbar } from "notistack";
import { useState } from "react";
import { useNavigate } from "react-router";
import afterWrite from "../../utils/afterWrite";
import { isHandled } from "../../utils/apiErrors";

const TeamsPage = () => {
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();

  const [search, setSearch] = useState("");
  const [blockedTeam, setBlockedTeam] = useState(null);
  const [toDelete, setToDelete] = useState(null);
  const [deleting, setDeleting] = useState(false);
  const [newTeamOpen, setNewTeamOpen] = useState(false);
  const [creating, setCreating] = useState(false);

  const { user, hasClaim } = useCurrentUser();
  const canManage = hasClaim(Claim.TeamsManage);
  const canViewAll = hasClaim(Claim.TeamsViewAll);

  const query = useDebouncedValue(search);
  const { teams, total, loading, error, reload } = useTeams(query);

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

  const countLabel = loading
    ? "Loading…"
    : `${total} ${total === 1 ? "team" : "teams"}`;

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
    } catch (err) {
      if (isHandled(err)) {
        setNewTeamOpen(false);
        return;
      }
      enqueueSnackbar("The team could not be created.", {
        variant: "toast",
        tone: "error",
        title: "Something went wrong",
      });
    } finally {
      setCreating(false);
    }
  };

  const handleDelete = (team) => {
    if (team.projects > 0) {
      setBlockedTeam(team);
      return;
    }
    setToDelete(team);
  };

  const confirmDelete = async () => {
    setDeleting(true);
    try {
      await deleteTeam(toDelete.id);
    } catch (err) {
      if (isHandled(err)) {
        setToDelete(null);
        return;
      }
      enqueueSnackbar(`${toDelete.name} could not be deleted.`, {
        variant: "toast",
        tone: "error",
        title: "Something went wrong",
      });
      return;
    } finally {
      setDeleting(false);
    }
    const name = toDelete.name;
    setToDelete(null);
    enqueueSnackbar(`${name} was deleted.`, {
      variant: "toast",
      tone: "success",
      title: "Team deleted",
    });
    await afterWrite(reload);
  };

  return (
    <>
      <PageHeader
        title="Teams"
        subtitle="All teams in the organisation"
        actions={
          canManage && (
            <AppButton onClick={() => setNewTeamOpen(true)}>New team</AppButton>
          )
        }
      />
      <PageBody>
        <PageToolbar count={countLabel}>
          <TextField
            type="search"
            placeholder="Search teams"
            fullWidth={false}
            sx={{ width: { xs: "100%", sm: "320px" } }}
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
        </PageToolbar>

        {error ? (
          <ErrorState
            message="The teams could not be loaded."
            onRetry={reload}
          />
        ) : (
          <TeamsTable
            teams={teams}
            canManage={canManage}
            onEdit={(team) => navigate(`/teams/${team.id}`)}
            onDelete={handleDelete}
            onOpen={(team) => navigate(`/teams/${team.id}`)}
            emptyTitle={emptyTitle}
            emptyText={emptyText}
            loading={loading}
          />
        )}
      </PageBody>

      <AppDialog
        open={Boolean(toDelete)}
        title="Delete team"
        onClose={deleting ? undefined : () => setToDelete(null)}
        actions={
          <>
            <AppButton
              variant="secondary"
              onClick={() => setToDelete(null)}
              disabled={deleting}
            >
              Cancel
            </AppButton>
            <AppButton
              variant="danger"
              onClick={confirmDelete}
              disabled={deleting}
            >
              {deleting ? "Deleting…" : "Delete team"}
            </AppButton>
          </>
        }
      >
        {toDelete?.name} and its memberships will be deleted. This cannot be
        undone.
      </AppDialog>

      <AppDialog
        open={Boolean(blockedTeam)}
        title="Team cannot be deleted"
        onClose={() => setBlockedTeam(null)}
        actions={
          <>
            <AppButton variant="secondary" onClick={() => setBlockedTeam(null)}>
              Cancel
            </AppButton>
            <AppButton onClick={() => setBlockedTeam(null)}>Close</AppButton>
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
