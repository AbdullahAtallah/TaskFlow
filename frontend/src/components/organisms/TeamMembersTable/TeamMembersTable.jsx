import {
  Box,
  Stack,
  Table,
  TableBody,
  TableHead,
  TableRow,
} from "@mui/material";
import HeadCell from "../../atoms/HeadCell/HeadCell";
import BodyCell from "../../atoms/BodyCell/BodyCell";
import Tag from "../../atoms/Tag/Tag";
import TextLink from "../../atoms/TextLink/TextLink";
import UserNameCell from "../../molecules/UserNameCell/UserNameCell";
import TableStateRow from "../../molecules/TableStateRow/TableStateRow";

const TeamMembersTable = ({
  members,
  loading,
  canManage,
  removingId,
  onRemove,
}) => (
  <Box
    sx={{
      border: "1px solid #e5e7eb",
      borderRadius: "8px",
      overflow: "auto",
      backgroundColor: "#ffffff",
    }}
  >
    <Table sx={{ tableLayout: "fixed", minWidth: "760px" }}>
      <TableHead>
        <TableRow>
          <HeadCell width="32%">Name</HeadCell>
          <HeadCell width="43%">E-mail</HeadCell>
          <HeadCell width="14%">Role</HeadCell>
          <HeadCell />
        </TableRow>
      </TableHead>

      <TableBody>
        {members.length === 0 && (
          <TableStateRow
            colSpan={4}
            loading={loading}
            emptyTitle="No members yet"
            emptyText="Add people to this team to get started."
          />
        )}
        {members.map((member) => (
          <TableRow
            key={member.userId}
            sx={{ "&:last-child td": { borderBottom: 0 } }}
          >
            <BodyCell>
              <UserNameCell name={member.displayName} />
            </BodyCell>
            <BodyCell>{member.userName}</BodyCell>
            <BodyCell>
              <Stack direction="row" sx={{ gap: "6px", flexWrap: "wrap" }}>
                {member.roles.map((role) => (
                  <Tag key={role}>{role}</Tag>
                ))}
              </Stack>
            </BodyCell>
            <BodyCell>
              {canManage && (
                <Stack direction="row" sx={{ justifyContent: "flex-end" }}>
                  <TextLink
                    onClick={() => onRemove(member)}
                    disabled={removingId === member.userId}
                  >
                    {removingId === member.userId ? "Removing…" : "Remove"}
                  </TextLink>
                </Stack>
              )}
            </BodyCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  </Box>
);

export default TeamMembersTable;
