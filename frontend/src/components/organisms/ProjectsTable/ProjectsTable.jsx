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
import TextLink from "../../atoms/TextLink/TextLink";
import ClampText from "../../atoms/ClampText/ClampText";
import SortLabel from "../../molecules/SortLabel/SortLabel";
import { formatDate } from "../../../utils";

const ProjectsTable = ({ projects, loading, canManage, onDelete }) => {
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
            <HeadCell>
              <SortLabel active>Project</SortLabel>
            </HeadCell>
            <HeadCell width="150px">
              <SortLabel>Team</SortLabel>
            </HeadCell>
            <HeadCell width="190px">Description</HeadCell>
            <HeadCell width="120px">
              <SortLabel>Open tasks</SortLabel>
            </HeadCell>
            <HeadCell width="100px">Done</HeadCell>
            <HeadCell width="140px">
              <SortLabel>Created</SortLabel>
            </HeadCell>
            <HeadCell width="100px" />
          </TableRow>
        </TableHead>

        <TableBody>
          {projects.length === 0 && !loading && (
            <TableRow>
              <TableCell
                colSpan={7}
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
                  There are no projects for this team.
                </Typography>
              </TableCell>
            </TableRow>
          )}

          {projects.map((project) => (
            <TableRow
              key={project.id}
              sx={{
                "&:last-child td": { borderBottom: 0 },
                ...(project.hasOverdue && {
                  "& td:first-of-type": { boxShadow: "inset 3px 0 0 #eb7b20" },
                }),
              }}
            >
              <BodyCell bold>{project.name}</BodyCell>
              <BodyCell>{project.teamName}</BodyCell>
              <BodyCell>
                <ClampText lines={2}>{project.description}</ClampText>
              </BodyCell>
              <BodyCell>{project.open}</BodyCell>
              <BodyCell>
                {project.done} / {project.total}
              </BodyCell>
              <BodyCell>{formatDate(project.created)}</BodyCell>
              <BodyCell>
                {canManage && (
                  <TextLink onClick={() => onDelete?.(project)}>
                    Delete
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

export default ProjectsTable;
