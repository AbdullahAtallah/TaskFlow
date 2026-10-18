import { useState } from "react";
import { useNavigate, useParams } from "react-router";
import { Box, Stack, Typography } from "@mui/material";
import { useSnackbar } from "notistack";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import PageTabs from "../../components/molecules/PageTabs/PageTabs";
import FullPageLoader from "../../components/atoms/FullPageLoader/FullPageLoader";
import TeamGeneralCard from "../../components/organisms/TeamGeneralCard/TeamGeneralCard";
import TeamDangerCard from "../../components/organisms/TeamDangerCard/TeamDangerCard";
import TeamMembersTab from "../../components/organisms/TeamMembersTab/TeamMembersTab";
import TeamProjectsTable from "../../components/organisms/TeamProjectsTable/TeamProjectsTable";
import NotFoundState from "../../components/molecules/NotFoundState/NotFoundState";
import useTeam from "../../hooks/useTeam";
import useTeamProjects from "../../hooks/useTeamProjects";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { Claim } from "../../constants/permissions";
import { deleteTeam, updateTeam } from "../../api/teams";
import afterWrite from "../../utils/afterWrite";

const TeamDetailsPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();
  const { hasClaim } = useCurrentUser();
  const canManage = hasClaim(Claim.TeamsManage);
  const canManageMembers = hasClaim(Claim.TeamsMembersManage);

  const { team, loading, error, reload } = useTeam(id);
  const { projects, loading: projectsLoading } = useTeamProjects(id);
  const [tab, setTab] = useState("general");
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);

  if (loading && !team) return <FullPageLoader />;

  if (error?.response?.status === 404) {
    return (
      <PageBody>
        <NotFoundState
          title="Team not found"
          message="This team doesn't exist, or you're not a member of it."
          backTo="/teams"
          backLabel="Back to teams"
        />
      </PageBody>
    );
  }

  if (error || !team) {
    return (
      <PageBody>
        <Typography sx={{ fontSize: 14, color: "#dc2626" }}>
          This team could not be loaded.
        </Typography>
      </PageBody>
    );
  }

  const handleSave = async ({ name, description }) => {
    setSaving(true);
    try {
      await updateTeam(team.id, {
        name,
        description,
        rowVersion: team.rowVersion,
      });
      await afterWrite(reload);
      enqueueSnackbar(`${name} was updated.`, {
        variant: "toast",
        tone: "success",
        title: "Team saved",
      });
    } catch {
      enqueueSnackbar("The team could not be saved.", {
        variant: "toast",
        tone: "error",
        title: "Something went wrong",
      });
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    setDeleting(true);
    try {
      await deleteTeam(team.id);
      enqueueSnackbar(`${team.name} was deleted.`, {
        variant: "toast",
        tone: "success",
        title: "Team deleted",
      });
      navigate("/teams", { replace: true });
    } catch {
      enqueueSnackbar(`${team.name} could not be deleted.`, {
        variant: "toast",
        tone: "error",
        title: "Something went wrong",
      });
      setDeleting(false);
    }
  };

  return (
    <>
      <PageHeader title={team.name} subtitle={team.description} />
      <PageBody>
        <PageTabs
          value={tab}
          onChange={setTab}
          tabs={[
            { value: "general", label: "General" },
            { value: "members", label: "Members", count: team.members },
            { value: "projects", label: "Projects", count: team.projects },
          ]}
        />

        <Box sx={{ mt: "24px" }}>
          {tab === "general" && (
            <Stack spacing="24px" sx={{ maxWidth: "645px" }}>
              <TeamGeneralCard
                team={team}
                canManage={canManage}
                saving={saving}
                onSave={handleSave}
              />
              {canManage && (
                <TeamDangerCard
                  team={team}
                  deleting={deleting}
                  onDelete={handleDelete}
                />
              )}
            </Stack>
          )}
          {tab === "members" && (
            <TeamMembersTab
              teamId={team.id}
              teamName={team.name}
              canManage={canManageMembers}
              onChanged={reload}
            />
          )}
          {tab === "projects" && (
            <TeamProjectsTable projects={projects} loading={projectsLoading} />
          )}
        </Box>
      </PageBody>
    </>
  );
};

export default TeamDetailsPage;
