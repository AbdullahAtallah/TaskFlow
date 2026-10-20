import { useEffect, useState } from "react";
import { Box, Button, Stack, Typography } from "@mui/material";
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
  members,
  task,
  saving,
  onSave,
  onClose,
}) => {
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
    setTitle(task?.title ?? "");
    setDescription(task?.description ?? "");
    setStatus(task ? STATUS_LABEL[task.status] : "To do");
    setPriority(task?.priority ?? "Medium");
    setDueText(task?.dueDate ? formatDate(task.dueDate) : "");
    setDueError("");
    setAssigneeId(task?.assigneeId ?? null);
    setTitleError("");
  }, [open, task]);

  const handleSave = () => {
    let hasError = false;

    if (!title.trim()) {
      setTitleError("Enter a title.");
      hasError = true;
    }

    const dueDate = dueText.trim() ? parseDate(dueText) : null;
    if (dueText.trim() && !dueDate) {
      setDueError("Enter a date like 14 okt 2026.");
      hasError = true;
    }

    if (hasError) return;

    onSave({
      title: title.trim(),
      description: description.trim() || null,
      status: STATUS[status],
      priority,
      dueDate,
      assigneeId,
    });
  };

  const isEdit = Boolean(task);

  return (
    <AppDialog
      open={open}
      title={isEdit ? "Edit task" : `New task in ${projectName}`}
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
            {saving ? "Saving…" : isEdit ? "Save changes" : "Create task"}
          </Button>
        </>
      }
    >
      <Stack spacing="22px">
        <Box>
          <FieldLabel>Title</FieldLabel>
          <TextField
            placeholder="e.g. Book elevator slots"
            value={title}
            autoFocus
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
            <Typography sx={{ fontSize: "14px", color: "#6b7280" }}>
              Members of {teamName}
            </Typography>
          </Stack>

          {members.length === 0 ? (
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
