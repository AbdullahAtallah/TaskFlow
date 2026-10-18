import { useState } from "react";
import { Button, Typography } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import PageToolbar from "../../components/molecules/PageToolbar/PageToolbar ";
import SegmentedTabs from "../../components/molecules/SegmentedTabs/SegmentedTabs";
import ProjectsTable from "../../components/organisms/ProjectsTable/ProjectsTable";
import useTeams from "../../hooks/useTeams";
import useProjects from "../../hooks/useProjects";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { Claim } from "../../constants/permissions";

const ALL_TEAMS = "All teams";

const ProjectsPage = () => {
  const [selectedTeam, setSelectedTeam] = useState(ALL_TEAMS);
  const { hasClaim } = useCurrentUser();
  const canCreate = hasClaim(Claim.ProjectsCreate);
  const canManage = hasClaim(Claim.ProjectsManage);

  const { teams } = useTeams("");
  const tabs = [ALL_TEAMS, ...teams.map((t) => t.name)];

  const teamId = teams.find((t) => t.name === selectedTeam)?.id ?? "all";
  const { projects, total, loading, error } = useProjects(teamId);

  const countLabel = loading
    ? "Loading…"
    : `${total} ${total === 1 ? "project" : "projects"}`;

  return (
    <>
      <PageHeader
        title="Projects"
        subtitle="Projects of every team

"
        actions={
          canCreate && (
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
          />
        )}
      </PageBody>
    </>
  );
};

export default ProjectsPage;
