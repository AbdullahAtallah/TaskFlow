import { Box } from "@mui/material";
import Card from "../../atoms/Card/Card";
import CardHeader from "../../molecules/CardHeader/CardHeader";
import StatTile from "../../molecules/StatTile/StatTile";

const TaskStatsCard = ({ stats }) => {
  return (
    <Card sx={{ display: "flex", flexDirection: "column" }}>
      <CardHeader title="Tasks per status" />

      <Box
        sx={{
          flex: 1,
          display: "grid",
          gridTemplateColumns: "1fr 1fr",
          gridTemplateRows: "1fr 1fr",
          "& > :nth-of-type(odd)": { borderRight: "1px solid #e5e7eb" },
          "& > :nth-of-type(-n+2)": { borderBottom: "1px solid #e5e7eb" },
        }}
      >
        <StatTile label="To do" value={stats.todo} />
        <StatTile label="In progress" value={stats.inProgress} />
        <StatTile label="Done" value={stats.done} color="#13790e" />
        <StatTile label="Overdue" value={stats.overdue} color="#e91b2a" />
      </Box>
    </Card>
  );
};

export default TaskStatsCard;
