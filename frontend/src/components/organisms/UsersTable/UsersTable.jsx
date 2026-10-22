import { Box, Table, TableBody, TableHead, TableRow } from "@mui/material";
import HeadCell from "../../atoms/HeadCell/HeadCell";
import BodyCell from "../../atoms/BodyCell/BodyCell";
import TextLink from "../../atoms/TextLink/TextLink";
import Tag from "../../atoms/Tag/Tag";
import SortLabel from "../../molecules/SortLabel/SortLabel";
import UserNameCell from "../../molecules/UserNameCell/UserNameCell";

const UsersTable = ({ users, onToggleActive, onSetPassword }) => {
  return (
    <Box
      sx={{
        border: "1px solid #e5e7eb",
        borderRadius: "8px",
        overflowX: "auto",
        backgroundColor: "#ffffff",
      }}
    >
      <Table sx={{ tableLayout: "fixed", minWidth: "1000px" }}>
        <TableHead>
          <TableRow>
            <HeadCell width="22%">
              <SortLabel active>Name</SortLabel>
            </HeadCell>
            <HeadCell width="19%">E-mail</HeadCell>
            <HeadCell width="13%">Roles</HeadCell>
            <HeadCell width="17%">Teams</HeadCell>
            <HeadCell width="10%">Status</HeadCell>
            <HeadCell width="19%" />
          </TableRow>
        </TableHead>

        <TableBody>
          {users.map((user) => (
            <TableRow
              key={user.id}
              sx={{
                "&:last-child td": { borderBottom: 0 },
              }}
            >
              <BodyCell>
                <UserNameCell name={user.name} />
              </BodyCell>
              <BodyCell>{user.email}</BodyCell>
              <BodyCell>
                <Box sx={{ display: "flex", flexWrap: "wrap", gap: "6px" }}>
                  {user.roles.map((role) => (
                    <Tag key={role}>{role}</Tag>
                  ))}
                </Box>
              </BodyCell>
              <BodyCell>{user.teams || "—"}</BodyCell>
              <BodyCell>
                {user.active ? (
                  <Tag tone="success">Active</Tag>
                ) : (
                  <Tag>Inactive</Tag>
                )}
              </BodyCell>
              <BodyCell>
                <Box
                  sx={{
                    display: "flex",
                    gap: "16px",
                    justifyContent: "flex-end",
                  }}
                >
                  <TextLink onClick={() => onSetPassword(user)}>
                    Set password
                  </TextLink>
                  {!user.isMe && (
                    <TextLink onClick={() => onToggleActive(user.id)}>
                      {user.active ? "Deactivate" : "Activate"}
                    </TextLink>
                  )}
                </Box>
              </BodyCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Box>
  );
};

export default UsersTable;
