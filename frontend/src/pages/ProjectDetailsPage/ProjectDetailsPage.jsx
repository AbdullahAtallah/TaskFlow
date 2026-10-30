import { useState } from "react";
import { useLocation, useNavigate, Navigate, useParams } from "react-router";
import { useSnackbar } from "notistack";

import Row from "../../components/atoms/Row/Row";
import Column from "../../components/atoms/Column/Column";
import EmptyState from "../../components/molecules/EmptyState/EmptyState";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import SegmentedTabs from "../../components/molecules/SegmentedTabs/SegmentedTabs";
import FullPageLoader from "../../components/atoms/FullPageLoader/FullPageLoader";
import NotFoundState from "../../components/molecules/NotFoundState/NotFoundState";
import ErrorState from "../../components/molecules/ErrorState/ErrorState";
import LoadingState from "../../components/molecules/LoadingState/LoadingState";
import AppButton from "../../components/atoms/AppButton/AppButton";
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
import { isHandled } from "../../utils/apiErrors";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { useBreadcrumbs } from "../../context/BreadcrumbContext";
import { Claim, Feature, Package } from "../../constants/permissions";

const VIEWS = ["List", "Board"];

const DEFAULT_FILTERS = {
  status: "All",
  priority: "All",
  assignee: "Everyone",
};

const ProjectDetailsPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user, hasClaim, hasPackage, hasFeature } = useCurrentUser();
  const { enqueueSnackbar } = useSnackbar();

  const [filters, setFilters] = useState(DEFAULT_FILTERS);
  const [taskDialogOpen, setTaskDialogOpen] = useState(false);
  const [editingTask, setEditingTask] = useState(null);
  const [savingTask, setSavingTask] = useState(false);
  const [toDelete, setToDelete] = useState(null);
  const [deleting, setDeleting] = useState(false);

  const canSeeTasks = hasPackage(Package.Tasks);
  const canCreateTask = canSeeTasks && hasClaim(Claim.TasksCreate);
  const canManageTasks = canSeeTasks && hasClaim(Claim.TasksManage);
  const canSeeTeams = hasPackage(Package.Teams);
  const canSeeBoard =
    canSeeTasks &&
    hasFeature(Feature.TasksBoard) &&
    hasClaim(Claim.TasksBoardView);
  const canAssign = hasClaim(Claim.TasksAssign);

  const { pathname } = useLocation();
  const onBoard = pathname.endsWith("/board");
  const view = onBoard ? "Board" : "List";

  const { project, loading, error, reload: reloadProject } = useProject(id);
  const {
    tasks: allTasks,
    loading: tasksLoading,
    error: tasksError,
    reload: reloadTasks,
  } = useProjectTasks(id, canSeeTasks);
  const {
    columns,
    loading: boardLoading,
    error: boardError,
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
  const shownError = onBoard ? boardError : tasksError;
  const countLabel = shownError
    ? ""
    : isLoading
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
        <ErrorState
          message="This project could not be loaded."
          onRetry={reloadProject}
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
    } catch (err) {
      setSavingTask(false);
      if (isHandled(err)) {
        setTaskDialogOpen(false);
        return;
      }
      showError(
        isEdit
          ? "The task could not be saved."
          : "The task could not be created.",
      );
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
    } catch (err) {
      setDeleting(false);
      if (isHandled(err)) {
        setToDelete(null);
        return;
      }
      showError(`${task.title} could not be deleted.`);
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
          <Row gap="24px">
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
            {canCreateTask && <AppButton onClick={openNew}>New task</AppButton>}
          </Row>
        }
      />

      <PageBody>
        {!canSeeTasks ? (
          <EmptyState title="You don't have access to tasks." />
        ) : (
          <Column gap="32px">
            <TaskFilters
              filters={filters}
              onChange={setFilters}
              countLabel={countLabel}
              showStatus={!onBoard}
            />

            {shownError ? (
              <ErrorState
                message="The tasks could not be loaded."
                onRetry={onBoard ? reloadBoard : reloadTasks}
              />
            ) : onBoard ? (
              boardLoading && columns.length === 0 ? (
                <LoadingState />
              ) : (
                <TaskBoard
                  columns={boardColumns}
                  onOpen={canManageTasks ? openEdit : undefined}
                />
              )
            ) : (
              <TasksTable
                tasks={tasks}
                loading={tasksLoading}
                canManage={canManageTasks}
                onEdit={openEdit}
                onDelete={setToDelete}
              />
            )}
          </Column>
        )}
      </PageBody>

      <TaskDialog
        open={taskDialogOpen}
        projectName={project.name}
        teamName={project.teamName}
        members={members}
        task={editingTask}
        saving={savingTask}
        onSave={handleSaveTask}
        canAssign={canAssign}
        onClose={() => setTaskDialogOpen(false)}
      />

      <AppDialog
        open={Boolean(toDelete)}
        title="Delete task"
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
              {deleting ? "Deleting…" : "Delete task"}
            </AppButton>
          </>
        }
      >
        "{toDelete?.title}" will be deleted.
      </AppDialog>
    </>
  );
};

export default ProjectDetailsPage;
