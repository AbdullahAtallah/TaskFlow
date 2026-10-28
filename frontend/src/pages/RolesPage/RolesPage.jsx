import { useNavigate } from "react-router";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import RolesTable from "../../components/organisms/RolesTable/RolesTable";
import ErrorState from "../../components/molecules/ErrorState/ErrorState";
import useRoles from "../../hooks/useRoles";

const RolePage = () => {
  const navigate = useNavigate();
  const { roles, loading, error, reload } = useRoles();

  return (
    <>
      <PageHeader title="Roles" subtitle="Permissions come only from roles" />
      <PageBody>
        {error ? (
          <ErrorState
            message="The roles could not be loaded."
            onRetry={reload}
          />
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
