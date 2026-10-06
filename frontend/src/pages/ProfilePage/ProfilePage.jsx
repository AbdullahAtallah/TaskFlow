import { Button } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";

const DashboardPage = () => {
  return (
    <>
      <PageHeader
        title="Profile"
        subtitle="Your account, teams and permissions"
      />
      <PageBody></PageBody>
    </>
  );
};

export default DashboardPage;
