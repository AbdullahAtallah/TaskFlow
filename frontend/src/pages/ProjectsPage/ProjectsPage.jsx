import { useState } from "react";
import { Typography, Box } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import PageToolbar from "../../components/molecules/PageToolbar/PageToolbar";
import SegmentedTabs from "../../components/molecules/SegmentedTabs/SegmentedTabs";
import AppButton from "../../components/atoms/AppButton/AppButton";
import ProjectsTable from "../../components/organisms/ProjectsTable/ProjectsTable";
import AppDialog from "../../components/organisms/AppDialog/AppDialog";
import NewProjectDialog from "../../components/organisms/NewProjectDialog/NewProjectDialog";
import useTeams from "../../hooks/useTeams";
import useProjects from "../../hooks/useProjects";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { Claim } from "../../constants/permissions";
import { useSnackbar } from "notistack";
import { useNavigate } from "react-router";
import { createProject, deleteProject } from "../../api/projects";
import afterWrite from "../../utils/afterWrite";

const ALL_TEAMS = "All teams";

const ProjectsPage = () => {
  const navigate = useNavigate();

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
      const rows = await afterWrite(reload);
      const created = rows?.find((p) => p.name === name && p.teamId === teamId);
      const teamName = teams.find((t) => t.id === teamId)?.name;

      enqueueSnackbar(`${name} was added to ${teamName}.`, {
        variant: "toast",
        tone: "success",
        title: "Project created",
        ...(created && {
          actionLabel: "View",
          onAction: () => navigate(`/projects/${created.id}`),
        }),
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
            <AppButton onClick={() => setNewOpen(true)}>New project</AppButton>
          )
        }
      />
      <PageBody>
        <PageToolbar count={countLabel}>
          <Box sx={{ overflowX: "auto", p: "2px", m: "-2px" }}>
            <SegmentedTabs
              options={tabs}
              value={selectedTeam}
              onChange={setSelectedTeam}
            />
          </Box>
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
            onOpen={(project) => navigate(`/projects/${project.id}`)}
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
              {deleting ? "Deleting…" : "Delete project"}
            </AppButton>
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
