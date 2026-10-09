import { Box, Stack, Typography } from "@mui/material";
import PriorityChip from "../../atoms/PriorityChip/PriorityChip";
import { Link as RouterLink } from "react-router";

const TaskItem = ({ title, project, due, overdue = false, priority }) => {
  return (
    <Box
      component={RouterLink}
      sx={{
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        gap: "16px",
        p: "12px 20px",
        borderBottom: "1px solid #e5e7eb",
        "&:last-of-type": { borderBottom: 0 },
        cursor: "pointer",
        "&:hover": { backgroundColor: "#fafbfc" },
        transition: "background-color 0.15s",
        textDecoration: "none",
      }}
    >
      <Box sx={{ minWidth: 0 }}>
        <Typography
          sx={{
            fontSize: "14px",
            fontWeight: 500,
            color: "#111827",
            lineHeight: "20px",
          }}
        >
          {title}
        </Typography>
        <Typography
          sx={{ fontSize: "13px", color: "#6b7280", lineHeight: "18px" }}
        >
          {project}
        </Typography>
      </Box>

      <Stack
        direction="row"
        sx={{ alignItems: "center", gap: "16px", flexShrink: 0 }}
      >
        <Typography
          sx={{
            fontSize: "13px",
            lineHeight: "18px",
            color: overdue ? "#e91b2a" : "#6b7280",
          }}
        >
          {due}
        </Typography>
        <PriorityChip priority={priority} />
      </Stack>
    </Box>
  );
};

export default TaskItem;
