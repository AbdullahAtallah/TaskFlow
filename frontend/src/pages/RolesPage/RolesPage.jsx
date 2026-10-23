import { Typography } from "@mui/material";
import { useNavigate } from "react-router";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import RolesTable from "../../components/organisms/RolesTable/RolesTable";
import useRoles from "../../hooks/useRoles";

const RolePage = () => {
  const navigate = useNavigate();
  const { roles, loading, error } = useRoles();

  return (
    <>
      <PageHeader title="Roles" subtitle="Permissions come only from roles" />
      <PageBody>
        {error ? (
          <Typography sx={{ fontSize: 14, color: "#dc2626" }}>
            Could not load roles. Please try again.
          </Typography>
        ) : (
          <RolesTable
            roles={roles}
            loading={loading}
            onOpen={(role) => navigate(`/management/roles/${role.id}`)}
          />
        )}
      </PageBody>
    </>
  );
};

export default RolePage;
