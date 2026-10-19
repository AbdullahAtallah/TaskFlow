import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import HeadCell from "../../atoms/HeadCell/HeadCell";
import BodyCell from "../../atoms/BodyCell/BodyCell";
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
          {projects.length === 0 && !loading && (
            <TableRow>
              <TableCell
                colSpan={4}
                sx={{ py: "48px", textAlign: "center", borderBottom: 0 }}
              >
                <Typography
                  sx={{ fontSize: "15px", fontWeight: 500, color: "#111827" }}
                >
                  No projects yet
                </Typography>
                <Typography
                  sx={{ mt: "4px", fontSize: "14px", color: "#6b7280" }}
                >
                  This team doesn't own any projects.
                </Typography>
              </TableCell>
            </TableRow>
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
