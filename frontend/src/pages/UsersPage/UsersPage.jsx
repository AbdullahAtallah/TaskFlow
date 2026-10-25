import { useState } from "react";
import { useSnackbar } from "notistack";
import { Typography } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import AppButton from "../../components/atoms/AppButton/AppButton";
import UsersTable from "../../components/organisms/UsersTable/UsersTable";
import NewUserDialog from "../../components/organisms/NewUserDialog/NewUserDialog";
import SetPasswordDialog from "../../components/organisms/SetPasswordDialog/SetPasswordDialog";
import useUsers from "../../hooks/useUsers";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { updateUser, createUser, setUserPassword } from "../../api/user";
import { getRoles } from "../../api/roles";
import afterWrite from "../../utils/afterWrite";

const ROLE_ORDER = ["Admin", "User", "ReadOnlyUser"];
const byRoleOrder = (a, b) =>
  (ROLE_ORDER.indexOf(a.name) + 1 || 99) -
  (ROLE_ORDER.indexOf(b.name) + 1 || 99);

const UsersPage = () => {
  const { enqueueSnackbar } = useSnackbar();
  const [busyId, setBusyId] = useState(null);

  const [newOpen, setNewOpen] = useState(false);
  const [creating, setCreating] = useState(false);
  const [roles, setRoles] = useState([]);

  const [passwordUser, setPasswordUser] = useState(null);
  const [savingPassword, setSavingPassword] = useState(false);

  const { user: me, signOut } = useCurrentUser();
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

  const openNew = async () => {
    document.activeElement?.blur();
    try {
      const r = await getRoles({ take: 100 });
      setRoles([...r.data].sort(byRoleOrder));
      setNewOpen(true);
    } catch {
      enqueueSnackbar("Roles could not be loaded.", {
        variant: "toast",
        tone: "error",
        title: "Something went wrong",
      });
    }
  };

  const handleCreate = async (data) => {
    setCreating(true);
    try {
      await createUser(data);
    } catch {
      enqueueSnackbar("The user could not be created.", {
        variant: "toast",
        tone: "error",
        title: "Something went wrong",
      });
      return;
    } finally {
      setCreating(false);
    }
    setNewOpen(false);
    enqueueSnackbar(`${data.displayName} can now sign in.`, {
      variant: "toast",
      tone: "success",
      title: "User created",
    });
    await afterWrite(reload);
  };

  const handleSetPassword = async (password) => {
    setSavingPassword(true);
    try {
      await setUserPassword(passwordUser.id, password);
    } catch {
      enqueueSnackbar(
        `The password for ${passwordUser.name} could not be set.`,
        {
          variant: "toast",
          tone: "error",
          title: "Something went wrong",
        },
      );
      return;
    } finally {
      if (passwordUser.isMe) {
        setPasswordUser(null);
        enqueueSnackbar("Sign in again with your new password.", {
          variant: "toast",
          tone: "success",
          title: "Password changed",
        });
        signOut();
        return;
      }

      enqueueSnackbar(
        `${passwordUser.name} can sign in with the new password.`,
        {
          variant: "toast",
          tone: "success",
          title: "Password set",
        },
      );
      setPasswordUser(null);
    }
  };

  return (
    <>
      <PageHeader
        title="Users"
        subtitle={subtitle}
        actions={<AppButton onClick={openNew}>New user</AppButton>}
      />
      <PageBody>
        {error ? (
          <Typography sx={{ fontSize: 14, color: "#dc2626" }}>
            Could not load users. Please try again.
          </Typography>
        ) : (
          <UsersTable
            users={rows}
            onToggleActive={toggleActive}
            onSetPassword={setPasswordUser}
          />
        )}
      </PageBody>
      <NewUserDialog
        open={newOpen}
        roles={roles}
        saving={creating}
        onCreate={handleCreate}
        onClose={() => setNewOpen(false)}
      />
      <SetPasswordDialog
        open={Boolean(passwordUser)}
        userName={passwordUser?.name}
        saving={savingPassword}
        onSave={handleSetPassword}
        onClose={() => setPasswordUser(null)}
      />
    </>
  );
};

export default UsersPage;
