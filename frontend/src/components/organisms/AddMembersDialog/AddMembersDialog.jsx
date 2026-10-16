import { useEffect, useState } from "react";
import { Box, Button, Typography } from "@mui/material";
import AppDialog from "../AppDialog/AppDialog";
import SelectableUserRow from "../../molecules/SelectableUserRow/SelectableUserRow";
import { getUsers } from "../../../api/user";

const AddMembersDialog = ({
  open,
  teamName,
  existingIds,
  saving,
  onAdd,
  onClose,
}) => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [selected, setSelected] = useState([]);

  useEffect(() => {
    if (!open) return;
    setSelected([]);
    setLoading(true);
    getUsers({ take: 100, sort: { field: "displayName", dir: "asc" } })
      .then((res) =>
        setUsers(res.data.filter((u) => !existingIds.includes(u.id))),
      )
      .finally(() => setLoading(false));
  }, [open, existingIds]);

  const toggle = (id) =>
    setSelected((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id],
    );

  return (
    <AppDialog
      open={open}
      title={`Add members to ${teamName}`}
      onClose={saving ? undefined : onClose}
      actions={
        <>
          <Button
            onClick={onClose}
            disabled={saving}
            sx={{
              textTransform: "none",
              height: "38px",
              px: "14px",
              borderRadius: "6px",
              border: "1px solid #e5e7eb",
              backgroundColor: "#ffffff",
              color: "#111827",
              fontWeight: 500,
              fontSize: 14,
            }}
          >
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={() => onAdd(selected)}
            disabled={selected.length === 0 || saving}
            sx={{
              textTransform: "none",
              height: "38px",
              px: "14px",
              borderRadius: "6px",
              backgroundColor: "#365aff",
              boxShadow: 0,
              fontWeight: 500,
              fontSize: 14,
            }}
          >
            {saving ? "Adding…" : "Add members"}
          </Button>
        </>
      }
    >
      <Box
        sx={{
          border: "1px solid #e5e7eb",
          borderRadius: "8px",
          maxHeight: "280px",
          overflowY: "auto",
        }}
      >
        {loading ? (
          <Typography sx={{ p: "18px", fontSize: 14, color: "#6b7280" }}>
            Loading…
          </Typography>
        ) : users.length === 0 ? (
          <Typography sx={{ p: "18px", fontSize: 14, color: "#6b7280" }}>
            Every user is already a member of this team.
          </Typography>
        ) : (
          users.map((user) => (
            <SelectableUserRow
              key={user.id}
              user={user}
              checked={selected.includes(user.id)}
              onToggle={() => toggle(user.id)}
            />
          ))
        )}
      </Box>
    </AppDialog>
  );
};

export default AddMembersDialog;
