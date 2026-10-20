import { Box, Stack, Typography } from "@mui/material";
import FieldLabel from "../../atoms/FieldLabel/FieldLabel";
import SegmentedTabs from "../../molecules/SegmentedTabs/SegmentedTabs";

export const STATUS_OPTIONS = ["All", "To do", "In progress", "Done"];
export const PRIORITY_OPTIONS = ["All", "High", "Medium", "Low"];
export const ASSIGNEE_OPTIONS = ["Everyone", "Me", "Unassigned"];

const Filter = ({ label, options, value, onChange }) => (
  <Box>
    <FieldLabel>{label}</FieldLabel>
    <SegmentedTabs
      options={options}
      value={value}
      onChange={onChange}
      ariaLabel={label}
    />
  </Box>
);

const TaskFilters = ({ filters, onChange, countLabel }) => {
  const set = (key) => (value) => onChange({ ...filters, [key]: value });

  return (
    <Stack spacing="18px">
      <Stack direction="row" sx={{ flexWrap: "wrap", gap: "24px" }}>
        <Box sx={{ flex: 1, minWidth: "320px" }}>
          <Filter
            label="Status"
            options={STATUS_OPTIONS}
            value={filters.status}
            onChange={set("status")}
          />
        </Box>
        <Box sx={{ flex: 1, minWidth: "320px" }}>
          <Filter
            label="Priority"
            options={PRIORITY_OPTIONS}
            value={filters.priority}
            onChange={set("priority")}
          />
        </Box>
      </Stack>

      <Stack
        direction="row"
        sx={{
          alignItems: "flex-end",
          justifyContent: "space-between",
          gap: "24px",
        }}
      >
        <Filter
          label="Assignee"
          options={ASSIGNEE_OPTIONS}
          value={filters.assignee}
          onChange={set("assignee")}
        />
        <Typography
          sx={{
            fontSize: "15px",
            lineHeight: "38px",
            color: "#6b7280",
            whiteSpace: "nowrap",
          }}
        >
          {countLabel}
        </Typography>
      </Stack>
    </Stack>
  );
};

export default TaskFilters;
