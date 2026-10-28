import { Divider, Stack } from "@mui/material";
import Card from "../../atoms/Card/Card";
import ProfileIdentity from "../../molecules/ProfileIdentity/ProfileIdentity";
import TagGroup from "../../molecules/TagGroup/TagGroup";
import AppButton from "../../atoms/AppButton/AppButton";
import { useCurrentUser } from "../../../context/CurrentUserContext";

const ProfileCard = ({ user }) => {
  const { signOut } = useCurrentUser();

  return (
    <Card sx={{ p: "24px" }}>
      <Stack
        spacing="16px"
        divider={<Divider sx={{ borderColor: "#e5e7eb" }} />}
      >
        <ProfileIdentity name={user.name} email={user.email} />

        <Stack spacing="16px">
          <TagGroup label="Roles" items={user.roles} tone="purple" />
          <TagGroup
            label="Teams"
            items={user.teams}
            emptyText={user.teamsEmptyText ?? "Not in any team yet"}
          />
        </Stack>

        <Stack direction="row">
          <AppButton variant="secondary" onClick={signOut}>
            Sign out
          </AppButton>
        </Stack>
      </Stack>
    </Card>
  );
};

export default ProfileCard;
