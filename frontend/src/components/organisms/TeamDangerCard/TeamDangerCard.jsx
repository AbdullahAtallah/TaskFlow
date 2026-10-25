import { Box, Stack, Typography } from "@mui/material";
import Card from "../../atoms/Card/Card";
import AppButton from "../../atoms/AppButton/AppButton";

const TeamDangerCard = ({ team, deleting, onDelete }) => {
  const blocked = team.projects > 0;
  const plural = team.projects === 1 ? "project" : "projects";

  return (
    <Card sx={{ p: "20px 24px" }}>
      <Stack
        direction="row"
        spacing="24px"
        sx={{ alignItems: "center", justifyContent: "space-between" }}
      >
        <Box>
          <Typography
            sx={{
              fontSize: "15px",
              fontWeight: 600,
              color: "#c70e0e",
              lineHeight: "22px",
            }}
          >
            Delete team
          </Typography>
          <Typography
            sx={{
              mt: "4px",
              fontSize: "14px",
              color: "#6b7280",
              lineHeight: "20px",
            }}
          >
            {blocked
              ? `This team still owns ${team.projects} active ${plural}. Move or delete them first.`
              : "This permanently deletes the team. Members keep their accounts."}
          </Typography>
        </Box>
        <AppButton
          variant="danger"
          onClick={onDelete}
          disabled={blocked || deleting}
          sx={{
            "&.Mui-disabled": { backgroundColor: "#f4b7bc", color: "#ffffff" },
          }}
        >
          {deleting ? "Deleting…" : "Delete team"}
        </AppButton>
      </Stack>
    </Card>
  );
};

export default TeamDangerCard;
