import { Button, Box } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import MyTasksCard from "../../components/organisms/MyTasksCard/MyTasksCard";
import TaskStatsCard from "../../components/organisms/TaskStatsCard/TaskStatsCard";
import ProjectsPerTeamCard from "../../components/organisms/ProjectsPerTeamCard/ProjectsPerTeamCard";
import MyTeamsCard from "../../components/organisms/MyTeamsCard/MyTeamsCard";

const DashboardPage = () => {
  const myTasks = [
    {
      id: 1,
      title: "Update access badges",
      project: "Office move Utrecht",
      due: "2 days overdue",
      overdue: true,
      priority: "High",
    },
    {
      id: 2,
      title: "Write homepage copy",
      project: "Website relaunch",
      due: "2 okt 2026",
      priority: "High",
    },
    {
      id: 3,
      title: "Request quotes from installers",
      project: "HVAC maintenance plan",
      due: "6 okt 2026",
      priority: "Medium",
    },
    {
      id: 4,
      title: "Label desks and lockers",
      project: "Office move Utrecht",
      due: "8 okt 2026",
      priority: "Low",
    },
    {
      id: 5,
      title: "Inform staff about parking",
      project: "Office move Utrecht",
      due: "10 okt 2026",
      priority: "Medium",
    },
    {
      id: 6,
      title: "Collect logo usage examples",
      project: "Brand guidelines",
      due: "15 okt 2026",
      priority: "Low",
    },
  ];

  const stats = { todo: 12, inProgress: 7, done: 5, overdue: 3 };

  const teams = [
    { id: 1, name: "Facilities", members: 5, projects: 3 },
    { id: 2, name: "Marketing", members: 3, projects: 2 },
    { id: 3, name: "IT Services", members: 3, projects: 2 },
    { id: 4, name: "Finance", members: 2, projects: 1 },
  ];

  const myTeams = teams.filter((team) => [1, 2].includes(team.id));
  return (
    <>
      <PageHeader
        title="Welcome back, Sara"
        subtitle="6 open tasks assigned to you · 30 sep 2026"
        actions={
          <>
            <Button
              variant="outlined"
              sx={{
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
              }}
            >
              New project
            </Button>
            <Button
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
              New task
            </Button>
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
          <MyTasksCard tasks={myTasks} />
          <TaskStatsCard stats={stats} />
          <ProjectsPerTeamCard teams={teams} />
          <MyTeamsCard teams={myTeams} />
        </Box>
      </PageBody>
    </>
  );
};

export default DashboardPage;
