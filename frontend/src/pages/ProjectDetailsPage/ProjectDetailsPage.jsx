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
import { createTask, deleteTask, getTask, updateTask } from "../../api/tasks";
import afterWrite from "../../utils/afterWrite";
import AppDialog from "../../components/organisms/AppDialog/AppDialog";
import { toTaskRow } from "../../api/tasks/mappers";

const VIEWS = ["List", "Board"];
const DEFAULT_FILTERS = {
  status: "All",
  priority: "All",
  assignee: "Everyone",
};

const primaryBtn = {
  textTransform: "none",
  backgroundColor: "#365aff",
  fontWeight: 500,
  fontSize: 14,
  p: "0 12px",
  height: "38px",
  borderRadius: "6px",
  boxShadow: 0,
};
const secondaryBtn = {
  textTransform: "none",
  height: "38px",
  px: "14px",
  borderRadius: "6px",
  border: "1px solid #e5e7eb",
  backgroundColor: "#ffffff",
  color: "#111827",
  fontWeight: 500,
  fontSize: 14,
};
const dangerBtn = {
  textTransform: "none",
  height: "38px",
  px: "14px",
  borderRadius: "6px",
  backgroundColor: "#dc2626",
  boxShadow: 0,
  fontWeight: 500,
  fontSize: 14,
  "&:hover": { backgroundColor: "#b91c1c", boxShadow: 0 },
};

const ProjectDetailsPage = () => {
  const { id } = useParams();
  const { user, hasClaim, hasPackage } = useCurrentUser();
  const { enqueueSnackbar } = useSnackbar();

  const { project, loading, error } = useProject(id);
  const {
    tasks: allTasks,
    loading: tasksLoading,
    reload: reloadTasks,
  } = useProjectTasks(id);
  const { members } = useTeamMembers(project?.teamId);

  const [view, setView] = useState("List");
  const [filters, setFilters] = useState(DEFAULT_FILTERS);
  const [taskDialogOpen, setTaskDialogOpen] = useState(false);
  const [editingTask, setEditingTask] = useState(null);
  const [savingTask, setSavingTask] = useState(false);
  const [toDelete, setToDelete] = useState(null);
  const [deleting, setDeleting] = useState(false);

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

  const showError = (message) =>
    enqueueSnackbar(message, {
      variant: "toast",
      tone: "error",
      title: "Something went wrong",
    });

  const openNew = () => {
    setEditingTask(null);
    setTaskDialogOpen(true);
  };

  const openEdit = async (task) => {
    try {
      setEditingTask(toTaskRow(await getTask(task.id)));
      setTaskDialogOpen(true);
    } catch {
      showError("This task could not be loaded.");
    }
  };

  const handleSaveTask = async (values) => {
    setSavingTask(true);
    try {
      if (editingTask) {
        await updateTask(editingTask.id, {
          ...values,
          rowVersion: editingTask.rowVersion,
        });
      } else {
        await createTask({ projectId: id, ...values });
      }
      await afterWrite(reloadTasks);

      enqueueSnackbar(
        editingTask
          ? `${values.title} was updated.`
          : `${values.title} was added to ${project.name}.`,
        {
          variant: "toast",
          tone: "success",
          title: editingTask ? "Task updated" : "Task created",
        },
      );
      setTaskDialogOpen(false);
    } catch {
      showError(
        editingTask
          ? "The task could not be saved."
          : "The task could not be created.",
      );
    } finally {
      setSavingTask(false);
    }
  };

  const confirmDelete = async () => {
    setDeleting(true);
    try {
      await deleteTask(toDelete.id);
      await afterWrite(reloadTasks);
      enqueueSnackbar(`${toDelete.title} was deleted.`, {
        variant: "toast",
        tone: "success",
        title: "Task deleted",
      });
      setToDelete(null);
    } catch {
      showError(`${toDelete.title} could not be deleted.`);
    } finally {
      setDeleting(false);
    }
  };

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
              <Button variant="contained" onClick={openNew} sx={primaryBtn}>
                New task
              </Button>
            )}
          </Stack>
        }
      />
      <PageBody>
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
              onEdit={openEdit}
              onDelete={setToDelete}
            />
          </Stack>
        ) : (
          <Typography sx={{ fontSize: "14px", color: "#6b7280" }}>
            Board comes later in phase 7.
          </Typography>
        )}
      </PageBody>
      <AppDialog
        open={Boolean(toDelete)}
        title="Delete task"
        onClose={deleting ? undefined : () => setToDelete(null)}
        actions={
          <>
            <Button
              onClick={() => setToDelete(null)}
              disabled={deleting}
              sx={secondaryBtn}
            >
              Cancel
            </Button>
            <Button
              variant="contained"
              onClick={confirmDelete}
              disabled={deleting}
              sx={dangerBtn}
            >
              {deleting ? "Deleting…" : "Delete task"}
            </Button>
          </>
        }
      >
        "{toDelete?.title}" will be deleted.
      </AppDialog>
      <TaskDialog
        open={taskDialogOpen}
        projectName={project.name}
        teamName={project.teamName}
        members={members}
        task={editingTask}
        saving={savingTask}
        onSave={handleSaveTask}
        onClose={() => setTaskDialogOpen(false)}
      />
    </>
  );
};

export default ProjectDetailsPage;
