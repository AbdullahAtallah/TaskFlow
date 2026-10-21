import { ButtonBase, Stack, Typography } from "@mui/material";
import PriorityChip from "../../atoms/PriorityChip/PriorityChip";
import UserAvatar from "../../atoms/UserAvatar/UserAvatar";
import { formatDate } from "../../../utils";
import { daysOverdue } from "../../../utils/taskDates";

const BoardCard = ({ task, onClick }) => {
  const overdueDays = task.isOverdue
    ? Math.max(1, daysOverdue(task.dueDate, task.status))
    : 0;

  return (
    <ButtonBase
      onClick={onClick}
      disabled={!onClick}
      disableRipple
      sx={{
        display: "block",
        width: "100%",
        textAlign: "left",
        p: "18px 20px",
        backgroundColor: "#ffffff",
        border: "1px solid #e5e7eb",
        borderRadius: "8px",
        cursor: onClick ? "pointer" : "default",
        "&:hover": onClick
          ? {
              borderColor: "rgb(209, 213, 219)",
              boxShadow: "0 2px 6px rgba(15, 29, 99, 0.06)",
            }
          : {},
      }}
    >
      <Typography
        sx={{
          fontSize: "14px",
          lineHeight: "24px",
          fontWeight: 500,
          color: "#111827",
        }}
      >
        {task.title}
      </Typography>

      <Stack
        direction="row"
        spacing="12px"
        sx={{ mt: "12px", alignItems: "center" }}
      >
        <PriorityChip priority={task.priority} />

        {overdueDays > 0 ? (
          <Typography sx={{ fontSize: "12px", color: "#e91b2a" }}>
            {overdueDays} {overdueDays === 1 ? "day" : "days"} overdue
          </Typography>
        ) : task.dueDate ? (
          <Typography sx={{ fontSize: "14px", color: "#6b7280" }}>
            {formatDate(task.dueDate)}
          </Typography>
        ) : null}

        {task.assigneeName && (
          <Stack sx={{ ml: "auto !important" }}>
            <UserAvatar
              name={task.assigneeName}
              size={24}
              sx={{ fontSize: "10px" }}
            />
          </Stack>
        )}
      </Stack>
    </ButtonBase>
  );
};

export default BoardCard;
