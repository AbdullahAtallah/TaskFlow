import { useEffect, useState } from "react";
import CardGrid from "../../components/atoms/CardGrid/CardGrid";
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
  const [teamsLoading, setTeamsLoading] = useState(canSeeTeams);
  const [teamsError, setTeamsError] = useState(false);

  useEffect(() => {
    if (!canSeeTeams) return;
    getTeams({ onlyMine: true, take: 100, sort: { field: "name", dir: "asc" } })
      .then((r) => {
        setTeams(r.data.map((t) => t.name));
        setTeamsError(false);
      })
      .catch(() => setTeamsError(true))
      .finally(() => setTeamsLoading(false));
  }, [canSeeTeams]);

  const profile = {
    name: user.displayName,
    email: user.userName,
    roles: toNames(user.roles),
    teams,
    teamsEmptyText: teamsLoading
      ? "Loading…"
      : teamsError
        ? "Teams could not be loaded."
        : "Not in any team yet",
  };

  return (
    <>
      <PageHeader
        title="Profile"
        subtitle="Your account, teams and permissions"
      />
      <PageBody>
        <CardGrid>
          <ProfileCard user={profile} />
          <PermissionsCard
            packages={sortByOrder(user.packages, Object.values(Package))}
            claims={sortByOrder(user.claims, Object.values(Claim))}
          />
        </CardGrid>
      </PageBody>
    </>
  );
};

export default ProfilePage;
