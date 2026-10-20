import { useState } from "react";
import { useParams } from "react-router";
import { Button, Stack, Typography } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import SegmentedTabs from "../../components/molecules/SegmentedTabs/SegmentedTabs";
import FullPageLoader from "../../components/atoms/FullPageLoader/FullPageLoader";
import TaskFilters from "../../components/organisms/TaskFilters/TaskFilters";
import TasksTable from "../../components/organisms/TasksTable/TasksTable";
import NotFoundState from "../../components/molecules/NotFoundState/NotFoundState";
import useProject from "../../hooks/useProject";
import useProjectTasks from "../../hooks/useProjectTasks";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { useBreadcrumbs } from "../../context/BreadcrumbContext";
import { Claim, Package } from "../../constants/permissions";
import filterTasks from "../../utils/filterTasks";
import { useSnackbar } from "notistack";
import TaskDialog from "../../components/organisms/TaskDialog/TaskDialog";
import useTeamMembers from "../../hooks/useTeamMembers";
import { createTask } from "../../api/tasks";
import afterWrite from "../../utils/afterWrite";

const VIEWS = ["List", "Board"];
const DEFAULT_FILTERS = {
  status: "All",
  priority: "All",
  assignee: "Everyone",
};

const ProjectDetailsPage = () => {
  const { id } = useParams();
  const { user, hasClaim, hasPackage } = useCurrentUser();

  const { project, loading, error } = useProject(id);
  const {
    tasks: allTasks,
    loading: tasksLoading,
    reload: reloadTasks,
  } = useProjectTasks(id);

  const { members } = useTeamMembers(project?.teamId);
  const { enqueueSnackbar } = useSnackbar();
  const [taskDialogOpen, setTaskDialogOpen] = useState(false);
  const [savingTask, setSavingTask] = useState(false);

  const [view, setView] = useState("List");
  const [filters, setFilters] = useState(DEFAULT_FILTERS);

  const canCreateTask = hasClaim(Claim.TasksCreate);
  const canManageTasks = hasClaim(Claim.TasksManage);
  const canSeeTeams = hasPackage(Package.Teams);

  const tasks = filterTasks(allTasks, filters, user.id);
  const countLabel = tasksLoading
    ? "Loading…"
    : `${tasks.length} ${tasks.length === 1 ? "task" : "tasks"}`;

  useBreadcrumbs(
    project
      ? [
          { label: "Projects", to: "/projects" },
          {
            label: project.teamName,
            to: canSeeTeams ? `/teams/${project.teamId}` : undefined,
          },
          { label: project.name },
        ]
      : null,
  );

  const handleCreateTask = async (values) => {
    setSavingTask(true);
    try {
      await createTask({ projectId: id, ...values });
      await afterWrite(reloadTasks);
      enqueueSnackbar(`${values.title} was added to ${project.name}.`, {
        variant: "toast",
        tone: "success",
        title: "Task created",
      });
      setTaskDialogOpen(false);
    } catch {
      enqueueSnackbar("The task could not be created.", {
        variant: "toast",
        tone: "error",
        title: "Something went wrong",
      });
    } finally {
      setSavingTask(false);
    }
  };

  if (loading && !project) return <FullPageLoader />;

  if (error?.response?.status === 404) {
    return (
      <PageBody>
        <NotFoundState
          title="Project not found"
          message="This project doesn't exist, or you're not a member of its team."
          backTo="/projects"
          backLabel="Back to projects"
        />
      </PageBody>
    );
  }

  if (error || !project) {
    return (
      <PageBody>
        <NotFoundState
          title="Something went wrong"
          message="This project could not be loaded. Please try again."
          backTo="/projects"
          backLabel="Back to projects"
        />
      </PageBody>
    );
  }

  const subtitle = project.description
    ? `${project.teamName} · ${project.description}`
    : project.teamName;

  return (
    <>
      <PageHeader
        title={project.name}
        subtitle={subtitle}
        actions={
          <Stack direction="row" spacing="24px" sx={{ alignItems: "center" }}>
            <SegmentedTabs
              options={VIEWS}
              value={view}
              onChange={setView}
              ariaLabel="View"
            />
            {canCreateTask && (
              <Button
                variant="contained"
                onClick={() => setTaskDialogOpen(true)}
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
                New task
              </Button>
            )}
          </Stack>
        }
      />
      <PageBody>
        {" "}
        {view === "List" ? (
          <Stack spacing="32px">
            <TaskFilters
              filters={filters}
              onChange={setFilters}
              countLabel={countLabel}
            />
            <TasksTable
              tasks={tasks}
              loading={tasksLoading}
              canManage={canManageTasks}
            />
          </Stack>
        ) : (
          <Typography sx={{ fontSize: "14px", color: "#6b7280" }}>
            Board comes later in phase 7.
          </Typography>
        )}
      </PageBody>
      <TaskDialog
        open={taskDialogOpen}
        projectName={project.name}
        teamName={project.teamName}
        members={members}
        task={null}
        saving={savingTask}
        onSave={handleCreateTask}
        onClose={() => setTaskDialogOpen(false)}
      />
    </>
  );
};

export default ProjectDetailsPage;
