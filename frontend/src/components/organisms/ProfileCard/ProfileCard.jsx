import { Divider, Stack, Button } from "@mui/material";
import { useNavigate } from "react-router";
import Card from "../../atoms/Card/Card";
import ProfileIdentity from "../../molecules/ProfileIdentity/ProfileIdentity";
import TagGroup from "../../molecules/TagGroup/TagGroup";

const ProfileCard = ({ user }) => {
  const navigate = useNavigate();

  const handleSignOut = () => {
    navigate("/sign-in");
  };

  return (
    <Card sx={{ p: "24px" }}>
      <Stack
        spacing="16px"
        divider={<Divider sx={{ borderColor: "#e5e7eb" }} />}
      >
        <ProfileIdentity name={user.name} email={user.email} />

        <Stack spacing="16px">
          <TagGroup label="Roles" items={user.roles} tone="purple" />
          <TagGroup label="Teams" items={user.teams} />
        </Stack>

        <Stack direction="row">
          <Button
            onClick={handleSignOut}
            sx={{
              textTransform: "none",
              backgroundColor: "#ffffff",
              color: "#111827",
              border: "1px solid #e5e7eb",
              fontWeight: 500,
              fontSize: 14,
              p: "0 12px",
              height: "38px",
              borderRadius: "6px",
              boxShadow: 0,
            }}
          >
            Sign out
          </Button>
        </Stack>
      </Stack>
    </Card>
  );
};

export default ProfileCard;
