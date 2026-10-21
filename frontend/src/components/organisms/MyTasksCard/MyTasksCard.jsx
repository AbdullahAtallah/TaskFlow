import { Typography } from "@mui/material";
import Card from "../../atoms/Card/Card";
import CardHeader from "../../molecules/CardHeader/CardHeader";
import TaskItem from "../../molecules/TaskItem/TaskItem";

const MyTasksCard = ({ tasks, total = tasks.length }) => {
  return (
    <Card>
      <CardHeader title="My tasks" meta={`${total} open`} />
      {tasks.length === 0 ? (
        <Typography sx={{ p: "16px 20px", fontSize: "14px", color: "#6b7280" }}>
          No open tasks assigned to you.
        </Typography>
      ) : (
        tasks.map((task) => <TaskItem key={task.id} {...task} />)
      )}
    </Card>
  );
};

export default MyTasksCard;
