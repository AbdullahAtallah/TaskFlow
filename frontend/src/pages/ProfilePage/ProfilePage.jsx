import { useEffect, useState } from "react";
import { Box } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import PermissionsCard from "../../components/organisms/PermissionsCard/PermissionsCard";
import ProfileCard from "../../components/organisms/ProfileCard/ProfileCard";
import { useCurrentUser } from "../../context/CurrentUserContext";
import toNames from "../../utils/toNames";
import sortByOrder from "../../utils/sortByOrder";
import { Package, Claim } from "../../constants/permissions";
import { getTeams } from "../../api/teams";

const ProfilePage = () => {
  const { user, hasPackage } = useCurrentUser();
  const canSeeTeams = hasPackage(Package.Teams);

  const [teams, setTeams] = useState([]);

  useEffect(() => {
    if (!canSeeTeams) return;
    getTeams({ onlyMine: true, take: 100, sort: { field: "name", dir: "asc" } })
      .then((r) => setTeams(r.data.map((t) => t.name)))
      .catch(() => setTeams([]));
  }, [canSeeTeams]);

  const profile = {
    name: user.displayName,
    email: user.userName,
    roles: toNames(user.roles),
    teams,
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
          <ProfileCard user={profile} />
          <PermissionsCard
            packages={sortByOrder(user.packages, Object.values(Package))}
            claims={sortByOrder(user.claims, Object.values(Claim))}
          />
        </Box>
      </PageBody>
    </>
  );
};

export default ProfilePage;
