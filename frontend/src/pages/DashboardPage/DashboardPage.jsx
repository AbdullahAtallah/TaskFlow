import { useState } from "react";
import { useNavigate } from "react-router";
import { Button, Box } from "@mui/material";
import { useSnackbar } from "notistack";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import MyTasksCard from "../../components/organisms/MyTasksCard/MyTasksCard";
import TaskStatsCard from "../../components/organisms/TaskStatsCard/TaskStatsCard";
import ProjectsPerTeamCard from "../../components/organisms/ProjectsPerTeamCard/ProjectsPerTeamCard";
import MyTeamsCard from "../../components/organisms/MyTeamsCard/MyTeamsCard";
import NewProjectDialog from "../../components/organisms/NewProjectDialog/NewProjectDialog";
import TaskDialog from "../../components/organisms/TaskDialog/TaskDialog";
import useDashboard from "../../hooks/useDashboard";
import useTeamMembers from "../../hooks/useTeamMembers";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { Claim, Package } from "../../constants/permissions";
import { formatDate } from "../../utils";
import afterWrite from "../../utils/afterWrite";
import { getTeams } from "../../api/teams";
import { toTeamRow } from "../../api/teams/mappers";
import { getProjects, createProject } from "../../api/projects";
import { toProjectRow } from "../../api/projects/mappers";
import { createTask } from "../../api/tasks";

const secondaryBtn = {
  textTransform: "none",
  backgroundColor: "#ffffff",
  color: "#111827",
  border: "1px solid #e5e7eb",
  fontWeight: 500,
  fontSize: 14,
  p: "0 12px",
  height: "38px",
  borderRadius: "6px",
  boxShadow: 0,
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

const BY_NAME = { take: 100, sort: { field: "name", dir: "asc" } };

const dueLabel = (task) => {
  if (!task.dueDate) return "No due date";
  if (!task.isOverdue) return formatDate(task.dueDate);

  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const due = new Date(task.dueDate);
  due.setHours(0, 0, 0, 0);
  const days = Math.round((today - due) / 86400000);
  return days === 1 ? "1 day overdue" : `${days} days overdue`;
};

const toTaskItem = (task) => ({
  id: task.id,
  title: task.title,
  project: task.projectName,
  due: dueLabel(task),
  overdue: task.isOverdue,
  priority: task.priority,
  to: `/projects/${task.projectId}`,
});

const toTeamBar = (row) => ({
  id: row.teamId,
  name: row.teamName,
  projects: row.projectCount,
});

const DashboardPage = () => {
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();

  const [dialog, setDialog] = useState(null);
  const [saving, setSaving] = useState(false);
  const [teams, setTeams] = useState([]);
  const [projects, setProjects] = useState([]);
  const [taskProjectId, setTaskProjectId] = useState(null);

  const { user, hasPackage, hasClaim } = useCurrentUser();
  const canSeeTeams = hasPackage(Package.Teams);
  const canSeeProjects = hasPackage(Package.Projects);
  const canCreateProject = hasClaim(Claim.ProjectsCreate);
  const canCreateTask = hasClaim(Claim.TasksCreate);

  const {
    myTasks,
    myTasksTotal,
    statusCounts,
    projectsPerTeam,
    myTeams,
    reload,
  } = useDashboard({ canSeeTeams, canSeeProjects });

  const taskProject = projects.find((p) => p.id === taskProjectId);
  const { members } = useTeamMembers(taskProject?.teamId);

  const firstName = (user?.displayName || "").split(" ")[0];
  const subtitle = `${myTasksTotal} open ${myTasksTotal === 1 ? "task" : "tasks"} assigned to you · ${formatDate(new Date())}`;

  const stats = {
    todo: statusCounts.toDo,
    inProgress: statusCounts.inProgress,
    done: statusCounts.done,
    overdue: statusCounts.overdue,
  };

  const teamBars = projectsPerTeam
    .map(toTeamBar)
    .sort((a, b) => b.projects - a.projects || a.name.localeCompare(b.name));

  const showSuccess = (title, message, viewPath) =>
    enqueueSnackbar(message, {
      variant: "toast",
      tone: "success",
      title,
      ...(viewPath && {
        actionLabel: "View",
        onAction: () => navigate(viewPath),
      }),
    });
  const showError = (message) =>
    enqueueSnackbar(message, {
      variant: "toast",
      tone: "error",
      title: "Something went wrong",
    });

  const closeDialog = () => {
    setDialog(null);
    setTaskProjectId(null);
  };

  const openNewProject = async () => {
    document.activeElement?.blur();
    try {
      const r = await getTeams(BY_NAME);
      setTeams(r.data.map(toTeamRow));
      setDialog("project");
    } catch {
      showError("Teams could not be loaded.");
    }
  };

  const openNewTask = async () => {
    document.activeElement?.blur();
    try {
      const r = await getProjects(BY_NAME);
      setProjects(r.data.map(toProjectRow));
      setTaskProjectId(null);
      setDialog("task");
    } catch {
      showError("Projects could not be loaded.");
    }
  };

  const handleCreateProject = async (data) => {
    const teamName = teams.find((t) => t.id === data.teamId)?.name;
    setSaving(true);
    try {
      await createProject(data);
    } catch {
      showError("The project could not be created.");
      return;
    } finally {
      setSaving(false);
    }
    closeDialog();
    await afterWrite(reload);

    const r = await getProjects({ teamId: data.teamId, ...BY_NAME }).catch(
      () => null,
    );
    const created = r?.data.find((p) => p.name === data.name);

    showSuccess(
      "Project created",
      `"${data.name}" was added to ${teamName}.`,
      created && `/projects/${created.id}`,
    );
  };

  const handleCreateTask = async ({ projectId, ...task }) => {
    const projectName = projects.find((p) => p.id === projectId)?.name;
    setSaving(true);
    try {
      await createTask({ projectId, ...task });
    } catch {
      showError("The task could not be created.");
      return;
    } finally {
      setSaving(false);
    }
    closeDialog();
    showSuccess(
      "Task created",
      `"${task.title}" was added to ${projectName}.`,
      `/projects/${projectId}`,
    );
    await afterWrite(reload);
  };

  return (
    <>
      <PageHeader
        title={`Welcome back, ${firstName}`}
        subtitle={subtitle}
        actions={
          <>
            {canCreateProject && (
              <Button
                variant="outlined"
                sx={secondaryBtn}
                onClick={openNewProject}
              >
                New project
              </Button>
            )}
            {canCreateTask && (
              <Button variant="contained" sx={primaryBtn} onClick={openNewTask}>
                New task
              </Button>
            )}
          </>
        }
      />
      <PageBody>
        <Box
          sx={{
            display: "grid",
            gridTemplateColumns: { xs: "1fr", lg: "1fr 1fr" },
            gap: "26px",
            pb: "20px",
          }}
        >
          <MyTasksCard tasks={myTasks.map(toTaskItem)} total={myTasksTotal} />
          <TaskStatsCard stats={stats} />
          {canSeeProjects && <ProjectsPerTeamCard teams={teamBars} />}
          {canSeeTeams && <MyTeamsCard teams={myTeams} />}
        </Box>
      </PageBody>

      <NewProjectDialog
        open={dialog === "project"}
        teams={teams}
        saving={saving}
        onCreate={handleCreateProject}
        onClose={closeDialog}
      />

      <TaskDialog
        open={dialog === "task"}
        projects={projects}
        onProjectChange={setTaskProjectId}
        members={members ?? []}
        saving={saving}
        onSave={handleCreateTask}
        onClose={closeDialog}
      />
    </>
  );
};

export default DashboardPage;
