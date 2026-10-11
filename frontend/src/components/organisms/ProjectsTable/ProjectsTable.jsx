import { Box, Table, TableBody, TableHead, TableRow } from "@mui/material";
import HeadCell from "../../atoms/HeadCell/HeadCell";
import BodyCell from "../../atoms/BodyCell/BodyCell";
import TextLink from "../../atoms/TextLink/TextLink";
import ClampText from "../../atoms/ClampText/ClampText";
import SortLabel from "../../molecules/SortLabel/SortLabel";

const ProjectsTable = ({ projects }) => {
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
          {projects.map((project) => (
            <TableRow
              key={project.id}
              sx={{
                "&:last-child td": { borderBottom: 0 },
              }}
            >
              <BodyCell bold>{project.name}</BodyCell>
              <BodyCell>{project.team}</BodyCell>
              <BodyCell>
                <ClampText lines={2}>{project.description}</ClampText>
              </BodyCell>
              <BodyCell>{project.openTasks}</BodyCell>
              <BodyCell>
                {project.done} / {project.total}
              </BodyCell>
              <BodyCell>{project.created}</BodyCell>
              <BodyCell>
                <TextLink>Delete</TextLink>
              </BodyCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Box>
  );
};

export default ProjectsTable;
