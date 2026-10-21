import { Box, Stack, Typography } from "@mui/material";
import NavCount from "../../atoms/NavCount/NavCount";
import BoardCard from "../../molecules/BoardCard/BoardCard";

const COLUMN_STYLE = {
  ToDo: { label: "To do", dot: "#9ca3af" },
  InProgress: { label: "In progress", dot: "#365aff" },
  Done: { label: "Done", dot: "#1ea618" },
};

const TaskBoard = ({ columns, onOpen }) => (
  <Box
    sx={{
      display: "grid",
      gridTemplateColumns: { xs: "1fr", md: "repeat(3, 1fr)" },
      gap: "16px",
      alignItems: "start",
    }}
  >
    {columns.map((column) => {
      const style = COLUMN_STYLE[column.status] ?? {
        label: column.status,
        dot: "#9ca3af",
      };

      return (
        <Box
          key={column.status}
          sx={{
            p: "12px",
            minHeight: "220px",
            backgroundColor: "#f9fafb",
            border: "1px solid #e5e7eb",
            borderRadius: "8px",
          }}
        >
          <Stack
            direction="row"
            spacing="8px"
            sx={{ alignItems: "center", mb: "16px", px: "4px" }}
          >
            <Box
              sx={{
                width: "8px",
                height: "8px",
                borderRadius: "50%",
                backgroundColor: style.dot,
              }}
            />
            <Typography
              sx={{
                fontSize: "16px",
                lineHeight: "24px",
                fontWeight: 500,
                color: "#111827",
              }}
            >
              {style.label}
            </Typography>
            <NavCount>{column.tasks.length}</NavCount>
          </Stack>

          <Stack spacing="8px">
            {column.tasks.map((task) => (
              <BoardCard
                key={task.id}
                task={task}
                onClick={onOpen ? () => onOpen(task) : undefined}
              />
            ))}
          </Stack>
        </Box>
      );
    })}
  </Box>
);

export default TaskBoard;
