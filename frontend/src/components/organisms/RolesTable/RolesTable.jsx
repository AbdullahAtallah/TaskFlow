import { Box, Table, TableBody, TableHead, TableRow } from "@mui/material";
import HeadCell from "../../atoms/HeadCell/HeadCell";
import BodyCell from "../../atoms/BodyCell/BodyCell";
import TableStateRow from "../../molecules/TableStateRow/TableStateRow";
import Tag from "../../atoms/Tag/Tag";

const RolesTable = ({ roles, loading, onOpen }) => {
  return (
    <Box
      sx={{
        border: "1px solid #e5e7eb",
        borderRadius: "8px",
        overflowX: "auto",
        backgroundColor: "#ffffff",
      }}
    >
      <Table sx={{ tableLayout: "fixed", minWidth: "800px" }}>
        <TableHead>
          <TableRow>
            <HeadCell width="20%">Role</HeadCell>
            <HeadCell width="41%">Description</HeadCell>
            <HeadCell width="10%">Packages</HeadCell>
            <HeadCell width="9%">Claims</HeadCell>
            <HeadCell width="9%">Users</HeadCell>
            <HeadCell width="11%" />
          </TableRow>
        </TableHead>

        <TableBody>
          {roles.length === 0 && (
            <TableStateRow
              colSpan={6}
              loading={loading}
              emptyTitle="No roles"
            />
          )}{" "}
          {roles.map((role) => (
            <TableRow
              key={role.id}
              onClick={onOpen ? () => onOpen(role) : undefined}
              sx={{
                "&:last-child td": { borderBottom: 0 },
                ...(onOpen && {
                  cursor: "pointer",
                  "&:hover": { backgroundColor: "#fafbfc" },
                }),
              }}
            >
              <BodyCell bold>{role.name}</BodyCell>
              <BodyCell>{role.description}</BodyCell>
              <BodyCell>{role.packages}</BodyCell>
              <BodyCell>{role.claims}</BodyCell>
              <BodyCell>{role.users}</BodyCell>
              <BodyCell>
                {role.isDefault && <Tag tone="info">Default</Tag>}
              </BodyCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Box>
  );
};

export default RolesTable;
