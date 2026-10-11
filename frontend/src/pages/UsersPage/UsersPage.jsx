import { Button } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import UsersTable from "../../components/organisms/UsersTable/UsersTable";
import { useState } from "react";

const initialUsers = [
  {
    id: 1,
    name: "Sara Jansen",
    email: "sara@example.com",
    role: "Admin",
    teams: "Facilities, Marketing",
    active: true,
    isMe: true,
  },
  {
    id: 2,
    name: "Diedrik de Vries",
    email: "diedrik@example.com",
    role: "Admin",
    teams: "Facilities, IT Services",
    active: true,
  },
  {
    id: 3,
    name: "Laura Bakker",
    email: "laura@example.com",
    role: "User",
    teams: "Facilities, Finance",
    active: true,
  },
  {
    id: 4,
    name: "Mees Visser",
    email: "mees@example.com",
    role: "User",
    teams: "Marketing",
    active: true,
  },
  {
    id: 5,
    name: "Noor Hendriks",
    email: "noor@example.com",
    role: "User",
    teams: "Facilities",
    active: true,
  },
  {
    id: 6,
    name: "Tim Smit",
    email: "tim@example.com",
    role: "User",
    teams: "Facilities, IT Services",
    active: true,
  },
  {
    id: 7,
    name: "Eva Mulder",
    email: "eva@example.com",
    role: "ReadOnlyUser",
    teams: "Marketing",
    active: true,
  },
  {
    id: 8,
    name: "Joris van Dam",
    email: "joris@example.com",
    role: "User",
    teams: "IT Services, Finance",
    active: false,
  },
];

const UsersPage = () => {
  const [users, setUsers] = useState(initialUsers);

  const toggleActive = (id) => {
    setUsers((prev) =>
      prev.map((user) =>
        user.id === id ? { ...user, active: !user.active } : user,
      ),
    );
  };

  return (
    <>
      <PageHeader
        title="Users"
        subtitle="8 users"
        actions={
          <>
            <Button
              variant="contained"
              sx={{
                textTransform: "none",
                backgroundColor: "#365aff",
                fontWeight: 500,
                fontSize: 14,
                p: "0 12px",
                height: "38px",
                borderRadius: "6px",
                boxShadow: 0,
              }}
            >
              New user
            </Button>
          </>
        }
      />
      <PageBody>
        <UsersTable users={users} onToggleActive={toggleActive} />
      </PageBody>
    </>
  );
};

export default UsersPage;
