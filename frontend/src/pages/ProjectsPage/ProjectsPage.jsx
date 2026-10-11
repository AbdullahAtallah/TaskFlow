import { Button } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import PageToolbar from "../../components/molecules/PageToolbar/PageToolbar ";
import SegmentedTabs from "../../components/molecules/SegmentedTabs/SegmentedTabs";
import ProjectsTable from "../../components/organisms/ProjectsTable/ProjectsTable";
import { useState } from "react";

const ALL_TEAMS = "All teams";
const tabs = [ALL_TEAMS, "Facilities", "Marketing", "IT Services", "Finance"];

const projects = [
  {
    id: 1,
    name: "Office move Utrecht",
    team: "Facilities",
    description:
      "Move 120 workplaces from Amersfoort to the new Utrecht office",
    openTasks: 6,
    done: 2,
    total: 8,
    created: "4 jun 2026",
  },
  {
    id: 2,
    name: "HVAC maintenance plan",
    team: "Facilities",
    description: "Yearly maintenance for all air handling units",
    openTasks: 2,
    done: 1,
    total: 3,
    created: "11 mei 2026",
  },
  {
    id: 3,
    name: "Fire safety audit",
    team: "Facilities",
    description: "Prepare for the external audit in October",
    openTasks: 2,
    done: 0,
    total: 2,
    created: "20 aug 2026",
  },
  {
    id: 4,
    name: "Website relaunch",
    team: "Marketing",
    description: "New marketing site for Q4",
    openTasks: 3,
    done: 1,
    total: 4,
    created: "1 jul 2026",
  },
  {
    id: 5,
    name: "Brand guidelines",
    team: "Marketing",
    description: "One reference for colours, type and logo use",
    openTasks: 1,
    done: 1,
    total: 2,
    created: "15 apr 2026",
  },
  {
    id: 6,
    name: "Laptop refresh 2026",
    team: "IT Services",
    description: "Replace laptops older than four years",
    openTasks: 2,
    done: 0,
    total: 2,
    created: "22 jun 2026",
  },
  {
    id: 7,
    name: "Access control upgrade",
    team: "IT Services",
    description: "New badge readers for all entrances",
    openTasks: 1,
    done: 0,
    total: 1,
    created: "2 sep 2026",
  },
  {
    id: 8,
    name: "Budget Q4",
    team: "Finance",
    description: "Collect and approve department budgets",
    openTasks: 2,
    done: 0,
    total: 2,
    created: "8 sep 2026",
  },
];
const ProjectsPage = () => {
  const [team, setTeam] = useState(ALL_TEAMS);

  const visibleProjects =
    team === ALL_TEAMS
      ? projects
      : projects.filter((project) => project.team === team);

  const countLabel = `${visibleProjects.length} ${visibleProjects.length === 1 ? "project" : "projects"}`;

  return (
    <>
      <PageHeader
        title="Projects"
        subtitle="Projects of every team

"
        actions={
          <>
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
              New project
            </Button>
          </>
        }
      />
      <PageBody>
        <PageToolbar count={countLabel}>
          <SegmentedTabs options={tabs} value={team} onChange={setTeam} />
        </PageToolbar>

        <ProjectsTable projects={visibleProjects} />
      </PageBody>
    </>
  );
};

export default ProjectsPage;
