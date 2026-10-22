import { useState } from "react";
import { useSnackbar } from "notistack";
import { Button, Typography } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import UsersTable from "../../components/organisms/UsersTable/UsersTable";
import useUsers from "../../hooks/useUsers";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { updateUser } from "../../api/user";
import afterWrite from "../../utils/afterWrite";

const primaryBtn = {
  textTransform: "none",
  backgroundColor: "#365aff",
  fontWeight: 500,
  fontSize: 14,
  p: "0 12px",
  height: "38px",
  borderRadius: "6px",
  boxShadow: 0,
};

const UsersPage = () => {
  const { enqueueSnackbar } = useSnackbar();
  const [busyId, setBusyId] = useState(null);

  const { user: me } = useCurrentUser();
  const { users, total, loading, error, reload } = useUsers();

  const rows = users
    .map((u) => ({ ...u, isMe: u.id === me.id }))
    .sort((a, b) => b.isMe - a.isMe);
  const subtitle = loading
    ? "Loading…"
    : `${total} ${total === 1 ? "user" : "users"}`;

  const toggleActive = async (id) => {
    if (busyId) return;
    const user = users.find((u) => u.id === id);
    const nowActive = !user.active;

    setBusyId(id);
    try {
      await updateUser(user.id, {
        displayName: user.name,
        isActive: nowActive,
        rowVersion: user.rowVersion,
      });
    } catch {
      enqueueSnackbar(`${user.name} could not be updated.`, {
        variant: "toast",
        tone: "error",
        title: "Something went wrong",
      });
      setBusyId(null);
      return;
    }

    enqueueSnackbar(
      nowActive
        ? `${user.name} can sign in again.`
        : `${user.name} can no longer sign in.`,
      {
        variant: "toast",
        tone: "success",
        title: nowActive ? "User activated" : "User deactivated",
      },
    );
    await afterWrite(reload);
    setBusyId(null);
  };

  return (
    <>
      <PageHeader
        title="Users"
        subtitle={subtitle}
        actions={
          <Button variant="contained" sx={primaryBtn}>
            New user
          </Button>
        }
      />
      <PageBody>
        {error ? (
          <Typography sx={{ fontSize: 14, color: "#dc2626" }}>
            Could not load users. Please try again.
          </Typography>
        ) : (
          <UsersTable users={rows} onToggleActive={toggleActive} />
        )}
      </PageBody>
    </>
  );
};

export default UsersPage;
