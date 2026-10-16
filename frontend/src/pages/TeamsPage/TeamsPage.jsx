import { Button, Typography } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import TextField from "../../components/molecules/TextField/TextField";
import TeamsTable from "../../components/organisms/TeamsTable/TeamsTable";
import PageToolbar from "../../components/molecules/PageToolbar/PageToolbar ";
import useTeams from "../../hooks/useTeams";
import useDebouncedValue from "../../hooks/useDebouncedValue";
import { useCurrentUser } from "../../context/CurrentUserContext";
import { Claim } from "../../constants/permissions";
import { useState } from "react";

const TeamsPage = () => {
  const [search, setSearch] = useState("");
  const query = useDebouncedValue(search);
  const { teams, total, loading, error, reload } = useTeams(query);
  const { hasClaim } = useCurrentUser();
  const canManage = hasClaim(Claim.TeamsManage);

  const countLabel = loading ? "Loading…" : `${total} teams`;
  return (
    <>
      <PageHeader
        title="Teams"
        subtitle="All teams in the organisation"
        actions={
          canManage && (
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
              New team
            </Button>
          )
        }
      />
      <PageBody>
        <PageToolbar count={countLabel}>
          <TextField
            type="search"
            placeholder="Search teams"
            fullWidth={false}
            sx={{ width: "320px" }}
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
        </PageToolbar>

        {error ? (
          <Typography sx={{ fontSize: 14, color: "#dc2626" }}>
            Could not load teams. Please try again.
          </Typography>
        ) : (
          <TeamsTable teams={teams} canManage={canManage} />
        )}
      </PageBody>
    </>
  );
};

export default TeamsPage;
