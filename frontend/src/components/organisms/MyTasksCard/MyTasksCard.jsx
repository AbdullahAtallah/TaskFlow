import Card from "../../atoms/Card/Card";
import CardHeader from "../../molecules/CardHeader/CardHeader";
import TaskItem from "../../molecules/TaskItem/TaskItem";

const MyTasksCard = ({ tasks }) => {
  return (
    <Card>
      <CardHeader title="My tasks" meta={`${tasks.length} open`} />
      {tasks.map((task) => (
        <TaskItem key={task.id} {...task} />
      ))}
    </Card>
  );
};

export default MyTasksCard;