import { Button, Box } from "@mui/material";
import { useNavigate } from "react-router";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import MyTasksCard from "../../components/organisms/MyTasksCard/MyTasksCard";
import TaskStatsCard from "../../components/organisms/TaskStatsCard/TaskStatsCard";
import ProjectsPerTeamCard from "../../components/organisms/ProjectsPerTeamCard/ProjectsPerTeamCard";
import MyTeamsCard from "../../components/organisms/MyTeamsCard/MyTeamsCard";
import useDashboard from "../../hooks/useDashboard";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { Claim, Package } from "../../constants/permissions";
import { formatDate } from "../../utils";

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

  const { user, hasPackage, hasClaim } = useCurrentUser();
  const canSeeTeams = hasPackage(Package.Teams);
  const canSeeProjects = hasPackage(Package.Projects);
  const canCreateProject = hasClaim(Claim.ProjectsCreate);
  const canCreateTask = hasClaim(Claim.TasksCreate);

  const { myTasks, myTasksTotal, statusCounts, projectsPerTeam, myTeams } =
    useDashboard({ canSeeTeams, canSeeProjects });

  const firstName = (
    user?.displayName ||
    user?.name ||
    user?.fullName ||
    ""
  ).split(" ")[0];
  const subtitle = `${myTasksTotal} open ${myTasksTotal === 1 ? "task" : "tasks"} assigned to you · ${formatDate(new Date())}`;

  const stats = {
    todo: statusCounts.toDo,
    inProgress: statusCounts.inProgress,
    done: statusCounts.done,
    overdue: statusCounts.overdue,
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
                onClick={() => navigate("/projects")}
              >
                New project
              </Button>
            )}
            {canCreateTask && (
              <Button
                variant="contained"
                sx={primaryBtn}
                onClick={() => navigate("/projects")}
              >
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
          {canSeeProjects && (
            <ProjectsPerTeamCard
              teams={projectsPerTeam
                .map(toTeamBar)
                .sort(
                  (a, b) =>
                    b.projects - a.projects || a.name.localeCompare(b.name),
                )}
            />
          )}
          {canSeeTeams && <MyTeamsCard teams={myTeams} />}
        </Box>
      </PageBody>
    </>
  );
};

export default DashboardPage;
