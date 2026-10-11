import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import RolesTable from "../../components/organisms/RolesTable/RolesTable";

const roles = [
  {
    id: 1,
    name: "Admin",
    description: "Full access, including users and roles",
    packages: 5,
    claims: 11,
    users: 2,
  },
  {
    id: 2,
    name: "User",
    description: "Works in teams, projects and tasks",
    packages: 4,
    claims: 6,
    users: 5,
    isDefault: true,
  },
  {
    id: 3,
    name: "ReadOnlyUser",
    description: "Read-only access to teams, projects and tasks",
    packages: 4,
    claims: 4,
    users: 1,
  },
];

const RolePage = () => {
  return (
    <>
      <PageHeader title="Roles" subtitle="Permissions come only from roles" />
      <PageBody>
        <RolesTable roles={roles} />
      </PageBody>
    </>
  );
};

export default RolePage;
