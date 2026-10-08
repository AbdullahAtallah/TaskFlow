import Card from "../../atoms/Card/Card";
import CardHeader from "../../molecules/CardHeader/CardHeader";
import TeamLinkItem from "../../molecules/TeamLinkItem/TeamLinkItem";

const MyTeamsCard = ({ teams }) => {
  return (
    <Card>
      <CardHeader title="My teams" />
      {teams.map((team) => (
        <TeamLinkItem
          key={team.id}
          name={team.name}
          members={team.members}
          projects={team.projects}
          to="/teams"
        />
      ))}
    </Card>
  );
};

export default MyTeamsCard;
