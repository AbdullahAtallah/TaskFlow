import { Box, Table, TableBody, TableHead, TableRow } from "@mui/material";
import HeadCell from "../../atoms/HeadCell/HeadCell";
import BodyCell from "../../atoms/BodyCell/BodyCell";
import Tag from "../../atoms/Tag/Tag";
import UserNameCell from "../../molecules/UserNameCell/UserNameCell";
import TableStateRow from "../../molecules/TableStateRow/TableStateRow";

const RoleUsersTable = ({ users }) => {
  return (
    <Box
      sx={{
        border: "1px solid #e5e7eb",
        borderRadius: "8px",
        overflowX: "auto",
        backgroundColor: "#ffffff",
      }}
    >
      <Table sx={{ tableLayout: "fixed", minWidth: "700px" }}>
        <TableHead>
          <TableRow>
            <HeadCell width="35%">Name</HeadCell>
            <HeadCell width="50%">E-mail</HeadCell>
            <HeadCell width="15%">Status</HeadCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {users.length === 0 && (
            <TableStateRow
              colSpan={3}
              emptyTitle="No users have this role yet."
            />
          )}
          {users.map((user) => (
            <TableRow
              key={user.id}
              sx={{ "&:last-child td": { borderBottom: 0 } }}
            >
              <BodyCell>
                <UserNameCell name={user.displayName} />
              </BodyCell>
              <BodyCell>{user.userName}</BodyCell>
              <BodyCell>
                {user.isActive ? (
                  <Tag tone="success">Active</Tag>
                ) : (
                  <Tag>Inactive</Tag>
                )}
              </BodyCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Box>
  );
};

export default RoleUsersTable;
