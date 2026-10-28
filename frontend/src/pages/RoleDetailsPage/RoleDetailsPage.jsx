import { useEffect, useState } from "react";
import { useParams } from "react-router";
import { useSnackbar } from "notistack";
import Column from "../../components/atoms/Column/Column";
import Row from "../../components/atoms/Row/Row";
import CardGrid from "../../components/atoms/CardGrid/CardGrid";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import PageTabs from "../../components/molecules/PageTabs/PageTabs";
import NavCount from "../../components/atoms/NavCount/NavCount";
import FullPageLoader from "../../components/atoms/FullPageLoader/FullPageLoader";
import NotFoundState from "../../components/molecules/NotFoundState/NotFoundState";
import ErrorState from "../../components/molecules/ErrorState/ErrorState";
import PackageCard from "../../components/organisms/PackageCard/PackageCard";
import RoleUsersTable from "../../components/organisms/RoleUsersTable/RoleUsersTable";
import useRole from "../../hooks/useRole";
import { updateRole } from "../../api/roles";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { useBreadcrumbs } from "../../context/BreadcrumbContext";
import { PERMISSION_CATALOG } from "../../constants/permissionCatalog";

const LEFT = ["General", "Projects", "Management"];
const RIGHT = ["Teams", "Tasks"];

const RoleDetailsPage = () => {
  const { id } = useParams();
  const { enqueueSnackbar } = useSnackbar();
  const { user: me, reloadUser } = useCurrentUser();

  const [tab, setTab] = useState("Permissions");
  const [packages, setPackages] = useState([]);
  const [claims, setClaims] = useState([]);

  const { role, users, loading, error, reload } = useRole(id);

  useEffect(() => {
    if (!role) return;
    setPackages(role.packages);
    setClaims(role.claims);
  }, [role]);

  useBreadcrumbs(
    role
      ? [
          { label: "Management" },
          { label: "Roles", to: "/management/roles" },
          { label: role.name },
        ]
      : null,
  );

  if (loading && !role) return <FullPageLoader />;

  if (error?.response?.status === 404) {
    return (
      <PageBody>
        <NotFoundState
          title="Role not found"
          message="This role doesn't exist."
          backTo="/management/roles"
          backLabel="Back to roles"
        />
      </PageBody>
    );
  }

  if (error || !role) {
    return (
      <PageBody>
        <ErrorState message="This role could not be loaded." onRetry={reload} />
      </PageBody>
    );
  }

  const isMyRole = me.roles.some((r) => (r.name ?? r) === role.name);
  const lockReason = (pkg) => {
    if (pkg.name !== "Management") return null;
    if (role.isDefault)
      return "The role for new users can't manage users and roles.";
    if (isMyRole && packages.includes("Management"))
      return "You can't remove your own access to users and roles.";
    return null;
  };
  const save = async (nextPackages, nextClaims, message) => {
    const prevPackages = packages;
    const prevClaims = claims;
    setPackages(nextPackages);
    setClaims(nextClaims);

    try {
      await updateRole(role.id, { packages: nextPackages, claims: nextClaims });
    } catch {
      setPackages(prevPackages);
      setClaims(prevClaims);
      enqueueSnackbar(`${role.name} could not be updated.`, {
        variant: "toast",
        tone: "error",
        title: "Something went wrong",
      });
      return;
    }

    enqueueSnackbar(message, {
      variant: "toast",
      tone: "success",
      title: "Role updated",
    });
    if (isMyRole) setTimeout(reloadUser, 600);
  };

  const togglePackage = (pkg, on) => {
    if (on) {
      save(
        [...packages, pkg.name],
        claims,
        `${pkg.name} was granted to ${role.name}.`,
      );
    } else {
      const names = pkg.claims.map((c) => c.name);
      save(
        packages.filter((p) => p !== pkg.name),
        claims.filter((c) => !names.includes(c)),
        `${pkg.name} was removed from ${role.name}.`,
      );
    }
  };

  const toggleClaim = (name, on) =>
    save(
      packages,
      on ? [...claims, name] : claims.filter((c) => c !== name),
      `${name} was ${on ? "granted to" : "removed from"} ${role.name}. It applies on the next request.`,
    );

  const renderColumn = (names) =>
    PERMISSION_CATALOG.filter((pkg) => names.includes(pkg.name)).map((pkg) => (
      <PackageCard
        key={pkg.name}
        pkg={pkg}
        enabled={pkg.alwaysOn || packages.includes(pkg.name)}
        locked={Boolean(lockReason(pkg))}
        lockedNote={lockReason(pkg)}
        claims={claims}
        onTogglePackage={togglePackage}
        onToggleClaim={toggleClaim}
      />
    ));

  const note = role.isDefault
    ? "Default role for new users, cannot be deleted"
    : role.isBuiltIn
      ? "Built-in role, cannot be deleted"
      : null;
  const subtitle = note ? `${role.description} · ${note}` : role.description;

  return (
    <>
      <PageHeader title={role.name} subtitle={subtitle} />
      <PageBody>
        <PageTabs
          tabs={[
            { value: "Permissions", label: "Permissions" },
            {
              value: "Assigned users",
              label: (
                <Row gap="8px">
                  <span>Assigned users</span>
                  <NavCount>{users.length}</NavCount>
                </Row>
              ),
            },
          ]}
          value={tab}
          onChange={setTab}
        />

        <Column sx={{ mt: "32px", pb: "20px" }}>
          {tab === "Permissions" ? (
            <CardGrid gap="32px">
              <Column gap="32px">{renderColumn(LEFT)}</Column>
              <Column gap="32px">{renderColumn(RIGHT)}</Column>
            </CardGrid>
          ) : (
            <RoleUsersTable users={users} />
          )}
        </Column>
      </PageBody>
    </>
  );
};

export default RoleDetailsPage;
