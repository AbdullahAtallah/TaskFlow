import { Typography } from "@mui/material";
import Card from "../../atoms/Card/Card";
import CardHeader from "../../molecules/CardHeader/CardHeader";
import TeamLinkItem from "../../molecules/TeamLinkItem/TeamLinkItem";

const MyTeamsCard = ({ teams }) => {
  return (
    <Card>
      <CardHeader title="My teams" />
      {teams.length === 0 ? (
        <Typography sx={{ p: "16px 20px", fontSize: "14px", color: "#6b7280" }}>
          You're not in any team yet.
        </Typography>
      ) : (
        teams.map((team) => (
          <TeamLinkItem
            key={team.id}
            name={team.name}
            members={team.members}
            projects={team.projects}
            to={`/teams/${team.id}`}
          />
        ))
      )}
    </Card>
  );
};

export default MyTeamsCard;
