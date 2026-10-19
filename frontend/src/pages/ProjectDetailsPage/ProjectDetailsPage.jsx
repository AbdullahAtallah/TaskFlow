import { useState } from "react";
import { useParams } from "react-router";
import { Button, Stack, Typography } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import SegmentedTabs from "../../components/molecules/SegmentedTabs/SegmentedTabs";
import FullPageLoader from "../../components/atoms/FullPageLoader/FullPageLoader";
import NotFoundState from "../../components/molecules/NotFoundState/NotFoundState";
import useProject from "../../hooks/useProject";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { Claim } from "../../constants/permissions";

const VIEWS = ["List", "Board"];

const ProjectDetailsPage = () => {
  const { id } = useParams();
  const { project, loading, error } = useProject(id);
  const { hasClaim } = useCurrentUser();
  const canCreateTask = hasClaim(Claim.TasksCreate);
  const [view, setView] = useState("List");

  if (loading && !project) return <FullPageLoader />;

  if (error?.response?.status === 404 || (!loading && !project && !error)) {
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

  if (error) {
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
      <PageBody></PageBody>
    </>
  );
};

export default ProjectDetailsPage;
