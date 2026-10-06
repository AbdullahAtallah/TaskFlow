import { Button } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";

const DashboardPage = () => {
  return (
    <>
      <PageHeader
        title="Roles"
        subtitle="Permissions come only from roles

"
      />
      <PageBody></PageBody>
    </>
  );
};

export default DashboardPage;
