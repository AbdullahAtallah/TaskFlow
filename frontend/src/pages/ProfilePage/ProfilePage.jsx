import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import { Box } from "@mui/material";
import PermissionsCard from "../../components/organisms/PermissionsCard/PermissionsCard";
import ProfileCard from "../../components/organisms/ProfileCard/ProfileCard";

const ProfilePage = () => {
  const currentUser = {
    name: "Sara Jansen",
    email: "sara@example.com",
    roles: ["Admin"],
    teams: ["Facilities", "Marketing"],
    packages: ["General", "Teams", "Projects", "Tasks", "Management"],
    claims: [
      "Teams.View.All",
      "Teams.Manage",
      "Teams.Members.Manage",
      "Projects.View.All",
      "Projects.Create",
      "Projects.Manage",
      "Tasks.View.All",
      "Tasks.Create",
      "Tasks.Manage",
      "Tasks.Assign",
      "Tasks.Board.View",
    ],
  };
  return (
    <>
      <PageHeader
        title="Profile"
        subtitle="Your account, teams and permissions"
      />
      <PageBody>
        <Box
          sx={{
            display: "grid",
            gridTemplateColumns: { xs: "1fr", lg: "1fr 1fr" },
            gap: "24px",
            alignItems: "start",
          }}
        >
          <ProfileCard user={currentUser} />
          <PermissionsCard
            packages={currentUser.packages}
            claims={currentUser.claims}
          />
        </Box>
      </PageBody>
    </>
  );
};

export default ProfilePage;
