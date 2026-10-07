import { Box, Stack, Typography } from "@mui/material";
import Card from "../../atoms/Card/Card";
import TagGroup from "../../molecules/TagGroup/TagGroup";
import ClaimList from "../../molecules/ClaimList/ClaimList";

const PermissionsCard = ({ packages, claims }) => {
  return (
    <Card sx={{ p: "24px" }}>
      <Stack spacing={"18px"}>
        <Box>
          <Typography
            component="h2"
            sx={{
              fontSize: "16px",
              fontWeight: 600,
              color: "#111827",
              m: 0,
              lineHeight: "24px",
            }}
          >
            Effective permissions
          </Typography>
          <Typography
            sx={{
              mt: "4px",
              fontSize: "13px",
              lineHeight: "18px",
              color: "#6b7280",
              textWrap: "pretty",
              letterSpacing: 0.4,
            }}
          >
            The union of the packages and claims of all your roles, from
            /api/user/current.
          </Typography>
        </Box>

        <TagGroup label="Packages" items={packages} />
        <ClaimList claims={claims} />
      </Stack>
    </Card>
  );
};

export default PermissionsCard;
