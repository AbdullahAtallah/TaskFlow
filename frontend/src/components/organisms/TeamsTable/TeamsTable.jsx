import { Box, Table, TableBody, TableHead, TableRow } from "@mui/material";
import HeadCell from "../../atoms/HeadCell/HeadCell";
import BodyCell from "../../atoms/BodyCell/BodyCell";
import SortLabel from "../../molecules/SortLabel/SortLabel";
import RowActions from "../../molecules/RowActions/RowActions";
import TableStateRow from "../../molecules/TableStateRow/TableStateRow";
import { formatDate } from "../../../utils";

const TeamsTable = ({
  teams,
  loading,
  canManage,
  onOpen,
  onDelete,
  onEdit,
  emptyTitle,
  emptyText,
}) => {
  return (
    <Box
      sx={{
        border: "1px solid #e5e7eb",
        borderRadius: "8px",
        overflow: "auto",
        backgroundColor: "#ffffff",
      }}
    >
      <Table sx={{ tableLayout: "fixed", minWidth: "960px" }}>
        <TableHead>
          <TableRow>
            <HeadCell width="22%">
              <SortLabel active>Name</SortLabel>
            </HeadCell>
            <HeadCell>Description</HeadCell>
            <HeadCell width="110px">
              <SortLabel>Members</SortLabel>
            </HeadCell>
            <HeadCell width="110px">
              <SortLabel>Projects</SortLabel>
            </HeadCell>
            <HeadCell width="140px">
              <SortLabel>Created</SortLabel>
            </HeadCell>
            <HeadCell width="140px" />
          </TableRow>
        </TableHead>

        <TableBody>
          {teams.length === 0 && (
            <TableStateRow
              colSpan={6}
              loading={loading}
              emptyTitle={emptyTitle}
              emptyText={emptyText}
            />
          )}

          {teams.map((team) => (
            <TableRow
              key={team.id}
              onClick={() => onOpen(team)}
              sx={{
                "&:last-child td": { borderBottom: 0 },
              }}
            >
              <BodyCell bold>{team.name}</BodyCell>
              <BodyCell>{team.description}</BodyCell>
              <BodyCell>{team.members}</BodyCell>
              <BodyCell>{team.projects}</BodyCell>
              <BodyCell>{formatDate(team.created)}</BodyCell>
              <BodyCell>
                <Box onClick={(e) => e.stopPropagation()}>
                  {canManage && (
                    <RowActions
                      onEdit={() => onEdit(team)}
                      onDelete={() => onDelete(team)}
                    />
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

export default TeamsTable;
