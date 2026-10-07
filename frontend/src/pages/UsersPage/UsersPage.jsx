import { Button } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";

const UsersPage = () => {
  return (
    <>
      <PageHeader
        title="Users"
        subtitle="8 users"
        actions={
          <>
            <Button
              variant="contained"
              sx={{
                textTransform: "none",
                backgroundColor: "#365aff",
                fontWeight: 500,
                fontSize: 14,
                p: "0 12px",
                height: "38px",
                borderRadius: "6px",
                boxShadow: 0,
              }}
            >
              New user
            </Button>
          </>
        }
      />
      <PageBody></PageBody>
    </>
  );
};

export default UsersPage;
