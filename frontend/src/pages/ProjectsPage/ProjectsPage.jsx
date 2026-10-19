import { useState } from "react";
import { Button, Typography } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import PageToolbar from "../../components/molecules/PageToolbar/PageToolbar ";
import SegmentedTabs from "../../components/molecules/SegmentedTabs/SegmentedTabs";
import ProjectsTable from "../../components/organisms/ProjectsTable/ProjectsTable";
import AppDialog from "../../components/organisms/AppDialog/AppDialog";
import NewProjectDialog from "../../components/organisms/NewProjectDialog/NewProjectDialog";
import useTeams from "../../hooks/useTeams";
import useProjects from "../../hooks/useProjects";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { Claim } from "../../constants/permissions";
import { useSnackbar } from "notistack";
import { createProject, deleteProject } from "../../api/projects";
import afterWrite from "../../utils/afterWrite";

const ALL_TEAMS = "All teams";

const ProjectsPage = () => {
  const [selectedTeam, setSelectedTeam] = useState(ALL_TEAMS);
  const { hasClaim } = useCurrentUser();
  const canCreate = hasClaim(Claim.ProjectsCreate);
  const canManage = hasClaim(Claim.ProjectsManage);

  const [toDelete, setToDelete] = useState(null);
  const [deleting, setDeleting] = useState(false);

  const [newOpen, setNewOpen] = useState(false);
  const [creating, setCreating] = useState(false);

  const { teams } = useTeams("");
  const tabs = [ALL_TEAMS, ...teams.map((t) => t.name)];

  const teamId = teams.find((t) => t.name === selectedTeam)?.id ?? "all";
  const { projects, total, loading, error, reload } = useProjects(teamId);
  const { enqueueSnackbar } = useSnackbar();

  const countLabel = loading
    ? "Loading…"
    : `${total} ${total === 1 ? "project" : "projects"}`;

  const confirmDelete = async () => {
    setDeleting(true);
    try {
      await deleteProject(toDelete.id);
      await afterWrite(reload);
      enqueueSnackbar(`${toDelete.name} and its tasks were deleted.`, {
        variant: "toast",
        tone: "success",
        title: "Project deleted",
      });
      setToDelete(null);
    } catch {
      enqueueSnackbar(`${toDelete.name} could not be deleted.`, {
        variant: "toast",
        tone: "error",
        title: "Something went wrong",
      });
    } finally {
      setDeleting(false);
    }
  };

  const handleCreate = async ({ teamId, name, description }) => {
    setCreating(true);
    try {
      await createProject({ teamId, name, description });
      await afterWrite(reload);

      const teamName = teams.find((t) => t.id === teamId)?.name;
      enqueueSnackbar(`${name} was added to ${teamName}.`, {
        variant: "toast",
        tone: "success",
        title: "Project created",
      });
      setNewOpen(false);
    } catch {
      enqueueSnackbar("The project could not be created.", {
        variant: "toast",
        tone: "error",
        title: "Something went wrong",
      });
    } finally {
      setCreating(false);
    }
  };

  return (
    <>
      <PageHeader
        title="Projects"
        subtitle="Projects of every team"
        actions={
          canCreate && (
            <Button
              onClick={() => setNewOpen(true)}
              variant="contained"
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
              New project
            </Button>
          )
        }
      />
      <PageBody>
        <PageToolbar count={countLabel}>
          <SegmentedTabs
            options={tabs}
            value={selectedTeam}
            onChange={setSelectedTeam}
          />
        </PageToolbar>
        {error ? (
          <Typography sx={{ fontSize: 14, color: "#dc2626" }}>
            Could not load projects. Please try again.
          </Typography>
        ) : (
          <ProjectsTable
            projects={projects}
            loading={loading}
            canManage={canManage}
            onDelete={setToDelete}
          />
        )}
      </PageBody>
      <AppDialog
        open={Boolean(toDelete)}
        title="Delete project"
        onClose={deleting ? undefined : () => setToDelete(null)}
        actions={
          <>
            <Button
              onClick={() => setToDelete(null)}
              disabled={deleting}
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
              onClick={confirmDelete}
              disabled={deleting}
              sx={{
                textTransform: "none",
                height: "38px",
                px: "14px",
                borderRadius: "6px",
                backgroundColor: "#dc2626",
                boxShadow: 0,
                fontWeight: 500,
                fontSize: 14,
                "&:hover": { backgroundColor: "#b91c1c", boxShadow: 0 },
              }}
            >
              {deleting ? "Deleting…" : "Delete project"}
            </Button>
          </>
        }
      >
        {toDelete?.name} and all of its tasks will be deleted. Deleted rows stay
        in the database with a deletion date.
      </AppDialog>
      <NewProjectDialog
        open={newOpen}
        teams={teams}
        defaultTeamName={selectedTeam === ALL_TEAMS ? undefined : selectedTeam}
        saving={creating}
        onCreate={handleCreate}
        onClose={() => setNewOpen(false)}
      />
    </>
  );
};

export default ProjectsPage;
