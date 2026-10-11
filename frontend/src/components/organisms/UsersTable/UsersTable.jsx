import { Box, Table, TableBody, TableHead, TableRow } from "@mui/material";
import HeadCell from "../../atoms/HeadCell/HeadCell";
import BodyCell from "../../atoms/BodyCell/BodyCell";
import TextLink from "../../atoms/TextLink/TextLink";
import Tag from "../../atoms/Tag/Tag";
import SortLabel from "../../molecules/SortLabel/SortLabel";
import UserNameCell from "../../molecules/UserNameCell/UserNameCell";

const UsersTable = ({ users, onToggleActive }) => {
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
            <HeadCell width="24%">
              <SortLabel active>Name</SortLabel>
            </HeadCell>
            <HeadCell width="20%">E-mail</HeadCell>
            <HeadCell width="14%">Roles</HeadCell>
            <HeadCell width="19%">Teams</HeadCell>
            <HeadCell width="12%">Status</HeadCell>
            <HeadCell width="11%" />
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
                <Tag>{user.role}</Tag>
              </BodyCell>
              <BodyCell>{user.teams}</BodyCell>
              <BodyCell>
                {user.active ? (
                  <Tag tone="success">Active</Tag>
                ) : (
                  <Tag>Inactive</Tag>
                )}
              </BodyCell>
              <BodyCell>
                {!user.isMe && (
                  <TextLink onClick={() => onToggleActive(user.id)}>
                    {user.active ? "Deactivate" : "Activate"}
                  </TextLink>
                )}
              </BodyCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Box>
  );
};

export default UsersTable;
