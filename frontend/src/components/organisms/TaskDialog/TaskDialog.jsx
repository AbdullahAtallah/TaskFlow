import { useEffect, useState } from "react";
import {
  Box,
  Button,
  MenuItem,
  Select,
  Stack,
  Typography,
} from "@mui/material";
import AppDialog from "../AppDialog/AppDialog";
import FieldLabel from "../../atoms/FieldLabel/FieldLabel";
import TextField from "../../molecules/TextField/TextField";
import SegmentedTabs from "../../molecules/SegmentedTabs/SegmentedTabs";
import AssigneeChip from "../../molecules/AssigneeChip/AssigneeChip";
import { parseDate } from "../../../utils/parseDate";
import { formatDate } from "../../../utils";

const STATUS = { "To do": "ToDo", "In progress": "InProgress", Done: "Done" };
const STATUS_LABEL = { ToDo: "To do", InProgress: "In progress", Done: "Done" };
const PRIORITIES = ["Low", "Medium", "High"];

const secondaryBtn = {
  textTransform: "none",
  height: "38px",
  px: "14px",
  borderRadius: "6px",
  border: "1px solid #e5e7eb",
  backgroundColor: "#ffffff",
  color: "#111827",
  fontWeight: 500,
  fontSize: 14,
};
const primaryBtn = {
  textTransform: "none",
  height: "38px",
  px: "14px",
  borderRadius: "6px",
  backgroundColor: "#365aff",
  boxShadow: 0,
  fontWeight: 500,
  fontSize: 14,
};
const selectSx = (hasError) => ({
  width: "100%",
  height: "38px",
  borderRadius: "6px",
  fontSize: 14,
  backgroundColor: "#ffffff",
  "& .MuiOutlinedInput-notchedOutline": {
    borderColor: hasError ? "#dc2626" : "#e5e7eb",
  },
  "&:hover .MuiOutlinedInput-notchedOutline": {
    borderColor: hasError ? "#dc2626" : "#d1d5db",
  },
  "&.Mui-focused .MuiOutlinedInput-notchedOutline": {
    borderColor: hasError ? "#dc2626" : "#365aff",
    borderWidth: "1px",
  },
});

const OptionalLabel = ({ children }) => (
  <Stack direction="row" sx={{ justifyContent: "space-between" }}>
    <FieldLabel>{children}</FieldLabel>
    <Typography sx={{ fontSize: "14px", color: "#9ca3af" }}>
      (optional)
    </Typography>
  </Stack>
);

const TaskDialog = ({
  open,
  projectName,
  teamName,
  members = [],
  task,
  saving,
  onSave,
  onClose,
  projects,
  onProjectChange,
  canAssign = true,
}) => {
  const pickProject = Array.isArray(projects);

  const [projectId, setProjectId] = useState("");
  const [projectError, setProjectError] = useState("");
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [status, setStatus] = useState("To do");
  const [priority, setPriority] = useState("Medium");
  const [dueText, setDueText] = useState("");
  const [dueError, setDueError] = useState("");
  const [assigneeId, setAssigneeId] = useState(null);
  const [titleError, setTitleError] = useState("");

  useEffect(() => {
    if (!open) return;
    setProjectId("");
    setProjectError("");
    setTitle(task?.title ?? "");
    setDescription(task?.description ?? "");
    setStatus(task ? STATUS_LABEL[task.status] : "To do");
    setPriority(task?.priority ?? "Medium");
    setDueText(task?.dueDate ? formatDate(task.dueDate) : "");
    setDueError("");
    setAssigneeId(task?.assigneeId ?? null);
    setTitleError("");
  }, [open, task]);

  const selectedProject = pickProject
    ? projects.find((p) => p.id === projectId)
    : null;
  const shownTeamName = pickProject ? selectedProject?.teamName : teamName;

  const changeProject = (id) => {
    setProjectId(id);
    setProjectError("");
    setAssigneeId(null); // members belong to the old team
    onProjectChange?.(id);
  };

  const handleSave = () => {
    let hasError = false;

    if (pickProject && !projectId) {
      setProjectError("Choose a project");
      hasError = true;
    }

    if (!title.trim()) {
      setTitleError("This field is required");
      hasError = true;
    }

    const dueDate = dueText.trim() ? parseDate(dueText) : null;
    if (dueText.trim() && !dueDate) {
      setDueError("Enter a date like 14 okt 2026.");
      hasError = true;
    }

    if (hasError) return;

    onSave({
      ...(pickProject && { projectId }),
      title: title.trim(),
      description: description.trim() || null,
      status: STATUS[status],
      priority,
      dueDate,
      assigneeId,
    });
  };

  const isEdit = Boolean(task);
  const dialogTitle = isEdit
    ? "Edit task"
    : pickProject
      ? "New task"
      : `New task in ${projectName}`;

  return (
    <AppDialog
      open={open}
      title={dialogTitle}
      onClose={saving ? undefined : onClose}
      actions={
        <>
          <Button onClick={onClose} disabled={saving} sx={secondaryBtn}>
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={handleSave}
            disabled={saving}
            sx={primaryBtn}
          >
            {saving ? "Saving…" : isEdit ? "Save" : "Create task"}
          </Button>
        </>
      }
    >
      <Stack spacing="22px">
        {pickProject && (
          <Box>
            <FieldLabel>Project</FieldLabel>
            {projects.length === 0 ? (
              <Typography sx={{ fontSize: "14px", color: "#dc2626" }}>
                Create a project first. A task always belongs to a project.
              </Typography>
            ) : (
              <>
                <Select
                  value={projectId}
                  onChange={(e) => changeProject(e.target.value)}
                  displayEmpty
                  sx={selectSx(Boolean(projectError))}
                  renderValue={(value) =>
                    value ? (
                      projects.find((p) => p.id === value)?.name
                    ) : (
                      <span style={{ color: "#9ca3af" }}>Choose a project</span>
                    )
                  }
                >
                  {projects.map((p) => (
                    <MenuItem key={p.id} value={p.id} sx={{ fontSize: 14 }}>
                      {p.name}
                      <Typography
                        component="span"
                        sx={{ ml: "8px", fontSize: "13px", color: "#6b7280" }}
                      >
                        {p.teamName}
                      </Typography>
                    </MenuItem>
                  ))}
                </Select>
                {projectError && (
                  <Typography
                    sx={{ mt: "6px", fontSize: "13px", color: "#dc2626" }}
                  >
                    {projectError}
                  </Typography>
                )}
              </>
            )}
          </Box>
        )}

        <Box>
          <FieldLabel>Title</FieldLabel>
          <TextField
            placeholder="e.g. Book elevator slots"
            value={title}
            autoFocus={!pickProject}
            onChange={(e) => {
              setTitle(e.target.value);
              setTitleError("");
            }}
            error={Boolean(titleError)}
            helperText={titleError}
          />
        </Box>

        <Box>
          <OptionalLabel>Description</OptionalLabel>
          <TextField
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
        </Box>

        <Box>
          <FieldLabel>Status</FieldLabel>
          <SegmentedTabs
            options={Object.keys(STATUS)}
            value={status}
            onChange={setStatus}
            ariaLabel="Status"
          />
        </Box>

        <Box>
          <FieldLabel>Priority</FieldLabel>
          <SegmentedTabs
            options={PRIORITIES}
            value={priority}
            onChange={setPriority}
            ariaLabel="Priority"
          />
        </Box>

        <Box>
          <OptionalLabel>Due date</OptionalLabel>
          <TextField
            placeholder="e.g. 14 okt 2026"
            value={dueText}
            onChange={(e) => {
              setDueText(e.target.value);
              setDueError("");
            }}
            error={Boolean(dueError)}
            helperText={dueError}
          />
        </Box>

        <Box>
          <Stack
            direction="row"
            spacing="10px"
            sx={{ alignItems: "baseline", mb: "10px" }}
          >
            <FieldLabel>Assignee</FieldLabel>
            {shownTeamName && (
              <Typography sx={{ fontSize: "14px", color: "#6b7280" }}>
                Members of {shownTeamName}
              </Typography>
            )}
          </Stack>

          {pickProject && !projectId ? (
            <Typography sx={{ fontSize: "14px", color: "#6b7280" }}>
              Choose a project first.
            </Typography>
          ) : !canAssign ? (
            <>
              <Typography sx={{ fontSize: "14px", color: "#111827" }}>
                {members.find((m) => m.userId === assigneeId)?.displayName ??
                  task?.assigneeName ??
                  "Unassigned"}
              </Typography>
              <Typography
                sx={{ mt: "4px", fontSize: "13px", color: "#9ca3af" }}
              >
                You don't have permission to assign tasks.
              </Typography>
            </>
          ) : members.length === 0 ? (
            <Typography sx={{ fontSize: "14px", color: "#6b7280" }}>
              This team has no members yet.
            </Typography>
          ) : (
            <Stack direction="row" sx={{ flexWrap: "wrap", gap: "8px" }}>
              {members.map((m) => (
                <AssigneeChip
                  key={m.userId}
                  name={m.displayName}
                  selected={assigneeId === m.userId}
                  onClick={() =>
                    setAssigneeId(assigneeId === m.userId ? null : m.userId)
                  }
                />
              ))}
            </Stack>
          )}
        </Box>
      </Stack>
    </AppDialog>
  );
};

export default TaskDialog;
