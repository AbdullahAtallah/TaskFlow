import { Stack } from "@mui/material";
import Card from "../../atoms/Card/Card";
import CardHeader from "../../molecules/CardHeader/CardHeader";
import TeamBarRow from "../../molecules/TeamBarRow/TeamBarRow";

const ProjectsPerTeamCard = ({ teams }) => {
  const max = Math.max(...teams.map((team) => team.projects), 0);

  return (
    <Card>
      <CardHeader title="Projects per team" />
      <Stack spacing="14px" sx={{ p: "16px 20px" }}>
        {teams.map((team) => (
          <TeamBarRow
            key={team.id}
            name={team.name}
            count={team.projects}
            max={max}
          />
        ))}
      </Stack>
    </Card>
  );
};

export default ProjectsPerTeamCard;
