import { Box, Table, TableBody, TableHead, TableRow } from "@mui/material";
import HeadCell from "../../atoms/HeadCell/HeadCell";
import BodyCell from "../../atoms/BodyCell/BodyCell";
import TextLink from "../../atoms/TextLink/TextLink";
import ClampText from "../../atoms/ClampText/ClampText";
import SortLabel from "../../molecules/SortLabel/SortLabel";
import TableStateRow from "../../molecules/TableStateRow/TableStateRow";
import { formatDate } from "../../../utils";

const ProjectsTable = ({ projects, loading, canManage, onOpen, onDelete }) => {
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
          {projects.length === 0 && (
            <TableStateRow
              colSpan={7}
              loading={loading}
              emptyTitle="No projects yet"
              emptyText="There are no projects for this team."
            />
          )}

          {projects.map((project) => (
            <TableRow
              key={project.id}
              onClick={() => onOpen(project)}
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
                  <TextLink
                    onClick={(e) => {
                      e.stopPropagation();
                      onDelete?.(project);
                    }}
                  >
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
