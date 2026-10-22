import { useState } from "react";
import { useLocation, useNavigate, Navigate, useParams } from "react-router";
import { Button, Stack } from "@mui/material";
import { useSnackbar } from "notistack";

import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import SegmentedTabs from "../../components/molecules/SegmentedTabs/SegmentedTabs";
import FullPageLoader from "../../components/atoms/FullPageLoader/FullPageLoader";
import NotFoundState from "../../components/molecules/NotFoundState/NotFoundState";
import AppDialog from "../../components/organisms/AppDialog/AppDialog";
import TaskFilters from "../../components/organisms/TaskFilters/TaskFilters";
import TasksTable from "../../components/organisms/TasksTable/TasksTable";
import TaskBoard from "../../components/organisms/TaskBoard/TaskBoard";
import TaskDialog from "../../components/organisms/TaskDialog/TaskDialog";

import useProject from "../../hooks/useProject";
import useProjectTasks from "../../hooks/useProjectTasks";
import useTaskBoard from "../../hooks/useTaskBoard";
import useTeamMembers from "../../hooks/useTeamMembers";
import { createTask, deleteTask, getTask, updateTask } from "../../api/tasks";
import { toTaskRow } from "../../api/tasks/mappers";

import filterTasks from "../../utils/filterTasks";
import afterWrite from "../../utils/afterWrite";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { useBreadcrumbs } from "../../context/BreadcrumbContext";
import { Claim, Feature, Package } from "../../constants/permissions";

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
  const navigate = useNavigate();
  const { user, hasClaim, hasPackage, hasFeature } = useCurrentUser();
  const { enqueueSnackbar } = useSnackbar();

  const [filters, setFilters] = useState(DEFAULT_FILTERS);
  const [taskDialogOpen, setTaskDialogOpen] = useState(false);
  const [editingTask, setEditingTask] = useState(null); // null = new task
  const [savingTask, setSavingTask] = useState(false);
  const [toDelete, setToDelete] = useState(null);
  const [deleting, setDeleting] = useState(false);

  const canCreateTask = hasClaim(Claim.TasksCreate);
  const canManageTasks = hasClaim(Claim.TasksManage);
  const canSeeTeams = hasPackage(Package.Teams);
  const canSeeBoard =
    hasFeature(Feature.TasksBoard) && hasClaim(Claim.TasksBoardView);
  const { pathname } = useLocation();
  const onBoard = pathname.endsWith("/board");
  const view = onBoard ? "Board" : "List";

  const { project, loading, error } = useProject(id);
  const {
    tasks: allTasks,
    loading: tasksLoading,
    reload: reloadTasks,
  } = useProjectTasks(id);
  const {
    columns,
    loading: boardLoading,
    reload: reloadBoard,
  } = useTaskBoard(id, onBoard && canSeeBoard);
  const { members } = useTeamMembers(project?.teamId);

  const tasks = filterTasks(allTasks, filters, user.id);

  const boardColumns = columns.map((column) => ({
    ...column,
    tasks: filterTasks(column.tasks, { ...filters, status: "All" }, user.id),
  }));
  const boardCount = boardColumns.reduce(
    (sum, column) => sum + column.tasks.length,
    0,
  );

  const shownCount = onBoard ? boardCount : tasks.length;
  const isLoading = onBoard ? boardLoading : tasksLoading;
  const countLabel = isLoading
    ? "Loading…"
    : `${shownCount} ${shownCount === 1 ? "task" : "tasks"}`;

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

  if (onBoard && !canSeeBoard) return <Navigate to="/dashboard" replace />;
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

  const showSuccess = (title, message) =>
    enqueueSnackbar(message, { variant: "toast", tone: "success", title });

  const reloadAll = async () => {
    await reloadTasks();
    if (onBoard) await reloadBoard();
  };

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
    const isEdit = Boolean(editingTask);
    setSavingTask(true);

    try {
      if (isEdit) {
        await updateTask(editingTask.id, {
          ...values,
          rowVersion: editingTask.rowVersion,
        });
      } else {
        await createTask({ projectId: id, ...values });
      }
    } catch {
      showError(
        isEdit
          ? "The task could not be saved."
          : "The task could not be created.",
      );
      setSavingTask(false);
      return;
    }

    setSavingTask(false);
    setTaskDialogOpen(false);
    showSuccess(
      isEdit ? "Task updated" : "Task created",
      isEdit
        ? `${values.title} was updated.`
        : `${values.title} was added to ${project.name}.`,
    );
    await afterWrite(reloadAll);
  };

  const confirmDelete = async () => {
    const task = toDelete;
    setDeleting(true);

    try {
      await deleteTask(task.id);
    } catch {
      showError(`${task.title} could not be deleted.`);
      setDeleting(false);
      return;
    }

    setDeleting(false);
    setToDelete(null);
    showSuccess("Task deleted", `${task.title} was deleted.`);
    await afterWrite(reloadAll);
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
            {canSeeBoard && (
              <SegmentedTabs
                options={VIEWS}
                value={view}
                onChange={(next) =>
                  navigate(
                    next === "Board"
                      ? `/projects/${id}/board`
                      : `/projects/${id}`,
                  )
                }
                ariaLabel="View"
              />
            )}
            {canCreateTask && (
              <Button variant="contained" onClick={openNew} sx={primaryBtn}>
                New task
              </Button>
            )}
          </Stack>
        }
      />

      <PageBody>
        <Stack spacing="32px">
          <TaskFilters
            filters={filters}
            onChange={setFilters}
            countLabel={countLabel}
            showStatus={!onBoard}
          />

          {onBoard ? (
            <TaskBoard
              columns={boardColumns}
              onOpen={canManageTasks ? openEdit : undefined}
            />
          ) : (
            <TasksTable
              tasks={tasks}
              loading={tasksLoading}
              canManage={canManageTasks}
              onEdit={openEdit}
              onDelete={setToDelete}
            />
          )}
        </Stack>
      </PageBody>

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
    </>
  );
};

export default ProjectDetailsPage;
