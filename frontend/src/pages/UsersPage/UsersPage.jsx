import { Button, Typography } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import UsersTable from "../../components/organisms/UsersTable/UsersTable";
import useUsers from "../../hooks/useUsers";
import { useCurrentUser } from "../../context/CurrentUserContext";

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
  const { user: me } = useCurrentUser();
  const { users, total, loading, error } = useUsers();

  const rows = users
    .map((u) => ({ ...u, isMe: u.id === me.id }))
    .sort((a, b) => b.isMe - a.isMe);
  const subtitle = loading
    ? "Loading…"
    : `${total} ${total === 1 ? "user" : "users"}`;

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
          <UsersTable users={rows} onToggleActive={() => {}} />
        )}
      </PageBody>
    </>
  );
};

export default UsersPage;
