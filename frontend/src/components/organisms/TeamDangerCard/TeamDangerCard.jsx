import { Box, Button, Stack, Typography } from "@mui/material";
import Card from "../../atoms/Card/Card";

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
        <Button
          variant="contained"
          onClick={onDelete}
          disabled={blocked || deleting}
          sx={{
            flexShrink: 0,
            textTransform: "none",
            height: "38px",
            px: "14px",
            borderRadius: "6px",
            backgroundColor: "#dc2626",
            boxShadow: 0,
            fontWeight: 500,
            fontSize: 14,
            "&.Mui-disabled": { backgroundColor: "#f4b7bc", color: "#ffffff" },
          }}
        >
          {deleting ? "Deleting…" : "Delete team"}
        </Button>
      </Stack>
    </Card>
  );
};

export default TeamDangerCard;
