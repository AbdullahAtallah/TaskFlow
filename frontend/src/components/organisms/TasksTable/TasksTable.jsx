import {
  Box,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import HeadCell from "../../atoms/HeadCell/HeadCell";
import BodyCell from "../../atoms/BodyCell/BodyCell";
import Tag from "../../atoms/Tag/Tag";
import PriorityChip from "../../atoms/PriorityChip/PriorityChip";
import SortLabel from "../../molecules/SortLabel/SortLabel";
import UserNameCell from "../../molecules/UserNameCell/UserNameCell";
import RowActions from "../../molecules/RowActions/RowActions";
import { formatDate } from "../../../utils";
import { daysOverdue } from "../../../utils/taskDates";

const statusTag = {
  ToDo: { label: "To do", tone: "info" },
  InProgress: { label: "In progress", tone: "purple" },
  Done: { label: "Done", tone: "success" },
};

const TasksTable = ({ tasks, loading, canManage, onEdit, onDelete }) => (
  <Box
    sx={{
      border: "1px solid #e5e7eb",
      borderRadius: "8px",
      overflowX: "auto",
      backgroundColor: "#ffffff",
    }}
  >
    <Table sx={{ tableLayout: "fixed", minWidth: "1000px" }}>
      <TableHead>
        <TableRow>
          <HeadCell width="30%">
            <SortLabel>Task</SortLabel>
          </HeadCell>
          <HeadCell width="13%">
            <SortLabel>Status</SortLabel>
          </HeadCell>
          <HeadCell width="11%">
            <SortLabel>Priority</SortLabel>
          </HeadCell>
          <HeadCell width="14%">
            <SortLabel active>Due date</SortLabel>
          </HeadCell>
          <HeadCell width="19%">Assignee</HeadCell>
          <HeadCell />
        </TableRow>
      </TableHead>

      <TableBody>
        {tasks.length === 0 && !loading && (
          <TableRow>
            <TableCell
              colSpan={6}
              sx={{ py: "48px", textAlign: "center", borderBottom: 0 }}
            >
              <Typography
                sx={{ fontSize: "15px", fontWeight: 500, color: "#111827" }}
              >
                No tasks found
              </Typography>
              <Typography
                sx={{ mt: "4px", fontSize: "14px", color: "#6b7280" }}
              >
                Try another filter, or create a new task.
              </Typography>
            </TableCell>
          </TableRow>
        )}

        {tasks.map((task) => {
          const status = statusTag[task.status] ?? {
            label: task.status,
            tone: "neutral",
          };
          const overdueDays = task.isOverdue
            ? Math.max(1, daysOverdue(task.dueDate, task.status))
            : 0;

          return (
            <TableRow
              key={task.id}
              onClick={canManage ? () => onEdit?.(task) : undefined}
              sx={{
                "&:last-child td": { borderBottom: 0 },
                ...(overdueDays > 0 && {
                  "& td:first-of-type": { boxShadow: "inset 3px 0 0 #eb7b20" },
                }),
              }}
            >
              <BodyCell bold>{task.title}</BodyCell>

              <BodyCell>
                <Tag tone={status.tone}>{status.label}</Tag>
              </BodyCell>

              <BodyCell>
                <PriorityChip priority={task.priority} />
              </BodyCell>

              <BodyCell>
                {task.dueDate ? formatDate(task.dueDate) : "—"}
                {overdueDays > 0 && (
                  <Typography
                    sx={{
                      fontSize: "14px",
                      lineHeight: "20px",
                      color: "#e91b2a",
                    }}
                  >
                    {overdueDays} {overdueDays === 1 ? "day" : "days"} overdue
                  </Typography>
                )}
              </BodyCell>

              <BodyCell>
                {task.assigneeName ? (
                  <UserNameCell name={task.assigneeName} />
                ) : (
                  <Typography sx={{ fontSize: "14px", color: "#9ca3af" }}>
                    Unassigned
                  </Typography>
                )}
              </BodyCell>

              <BodyCell>
                {canManage && (
                  <Box onClick={(e) => e.stopPropagation()}>
                    <Stack direction="row" sx={{ justifyContent: "flex-end" }}>
                      <RowActions
                        onEdit={() => onEdit?.(task)}
                        onDelete={() => onDelete?.(task)}
                      />
                    </Stack>
                  </Box>
                )}
              </BodyCell>
            </TableRow>
          );
        })}
      </TableBody>
    </Table>
  </Box>
);

export default TasksTable;
