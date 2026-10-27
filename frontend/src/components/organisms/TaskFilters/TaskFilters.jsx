import { Box, Stack, Typography } from "@mui/material";
import FieldLabel from "../../atoms/FieldLabel/FieldLabel";
import SegmentedTabs from "../../molecules/SegmentedTabs/SegmentedTabs";

export const STATUS_OPTIONS = ["All", "To do", "In progress", "Done"];
export const PRIORITY_OPTIONS = ["All", "High", "Medium", "Low"];
export const ASSIGNEE_OPTIONS = ["Everyone", "Me", "Unassigned"];

const Filter = ({ label, options, value, onChange }) => (
  <Box sx={{ minWidth: 0 }}>
    <FieldLabel>{label}</FieldLabel>
    <Box sx={{ overflowX: "auto", p: "2px", m: "-2px" }}>
      <SegmentedTabs
        options={options}
        value={value}
        onChange={onChange}
        ariaLabel={label}
      />
    </Box>
  </Box>
);

const Count = ({ children }) => (
  <Typography
    sx={{
      fontSize: "15px",
      lineHeight: "38px",
      color: "#6b7280",
      whiteSpace: "nowrap",
    }}
  >
    {children}
  </Typography>
);

const filterBox = {
  flex: { xs: "1 1 100%", sm: "1 1 0" },
  minWidth: { xs: 0, sm: "320px" },
};

const TaskFilters = ({ filters, onChange, countLabel, showStatus = true }) => {
  const set = (key) => (value) => onChange({ ...filters, [key]: value });

  const priority = (
    <Filter
      label="Priority"
      options={PRIORITY_OPTIONS}
      value={filters.priority}
      onChange={set("priority")}
    />
  );
  const assignee = (
    <Filter
      label="Assignee"
      options={ASSIGNEE_OPTIONS}
      value={filters.assignee}
      onChange={set("assignee")}
    />
  );

  if (!showStatus) {
    return (
      <Stack
        direction="row"
        sx={{ alignItems: "flex-end", flexWrap: "wrap", gap: "24px" }}
      >
        <Box sx={filterBox}>{priority}</Box>
        <Box sx={filterBox}>{assignee}</Box>
        <Count>{countLabel}</Count>
      </Stack>
    );
  }
  return (
    <Stack spacing="18px">
      <Stack direction="row" sx={{ flexWrap: "wrap", gap: "24px" }}>
        <Box sx={filterBox}>
          <Filter
            label="Status"
            options={STATUS_OPTIONS}
            value={filters.status}
            onChange={set("status")}
          />
        </Box>
        <Box sx={filterBox}>{priority}</Box>
      </Stack>

      <Stack
        direction="row"
        sx={{
          alignItems: "flex-end",
          justifyContent: "space-between",
          flexWrap: "wrap",
          gap: "24px",
        }}
      >
        {assignee}
        <Count>{countLabel}</Count>
      </Stack>
    </Stack>
  );
};
export default TaskFilters;
