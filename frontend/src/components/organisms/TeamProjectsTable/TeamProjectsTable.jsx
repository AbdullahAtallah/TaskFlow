import { Box, Table, TableBody, TableHead, TableRow } from "@mui/material";
import HeadCell from "../../atoms/HeadCell/HeadCell";
import BodyCell from "../../atoms/BodyCell/BodyCell";
import TableStateRow from "../../molecules/TableStateRow/TableStateRow";
import { formatDate } from "../../../utils";
import { useNavigate } from "react-router";

const TeamProjectsTable = ({ projects, loading }) => {
  const navigate = useNavigate();

  return (
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
            <HeadCell width="30%">Project</HeadCell>
            <HeadCell width="46%">Description</HeadCell>
            <HeadCell width="11%">Done</HeadCell>
            <HeadCell>Created</HeadCell>
          </TableRow>
        </TableHead>

        <TableBody>
          {projects.length === 0 && (
            <TableStateRow
              colSpan={4}
              loading={loading}
              emptyTitle="No projects yet"
              emptyText="This team doesn't own any projects."
            />
          )}

          {projects.map((project) => (
            <TableRow
              key={project.id}
              onClick={() => navigate(`/projects/${project.id}`)}
              sx={{ "&:last-child td": { borderBottom: 0 } }}
            >
              <BodyCell bold>{project.name}</BodyCell>
              <BodyCell>{project.description}</BodyCell>
              <BodyCell>{`${project.done} / ${project.total}`}</BodyCell>
              <BodyCell>{formatDate(project.created)}</BodyCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Box>
  );
};

export default TeamProjectsTable;
