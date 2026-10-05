import { Button } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";

const DashboardPage = () => {
  return (
    <>
      <PageHeader
        title="Welcome back, Sara"
        subtitle="6 open tasks assigned to you · 30 sep 2026"
        actions={
          <>
            <Button variant="outlined" sx={{ textTransform: "none" }}>
              New project
            </Button>
            <Button
              variant="contained"
              sx={{ textTransform: "none", backgroundColor: "#365aff" }}
            >
              New task
            </Button>
          </>
        }
      />
      <PageBody></PageBody>
    </>
  );
};

export default DashboardPage;
