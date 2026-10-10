import { Button } from "@mui/material";
import PageHeader from "../../components/molecules/PageHeader/PageHeader";
import PageBody from "../../components/atoms/PageBody/PageBody";
import TextField from "../../components/molecules/TextField/TextField";
import TeamsTable from "../../components/organisms/TeamsTable/TeamsTable";
import PageToolbar from "../../components/molecules/PageToolbar/PageToolbar ";
import { useState } from "react";

const teams = [
  {
    id: 1,
    name: "Facilities",
    description: "Building operations, maintenance and office moves",
    members: 5,
    projects: 3,
    created: "12 jan 2026",
  },
  {
    id: 2,
    name: "Marketing",
    description: "Brand, website and campaigns",
    members: 3,
    projects: 2,
    created: "3 feb 2026",
  },
  {
    id: 3,
    name: "IT Services",
    description: "Workplace hardware, access and software",
    members: 3,
    projects: 2,
    created: "18 feb 2026",
  },
  {
    id: 4,
    name: "Finance",
    description: "Budgeting and reporting",
    members: 2,
    projects: 1,
    created: "2 mrt 2026",
  },
];

const TeamsPage = () => {
  const [search, setSearch] = useState("");

  const query = search.trim().toLowerCase();
  const visibleTeams = teams.filter((team) =>
    team.name.toLowerCase().includes(query),
  );

  const countLabel = `${visibleTeams.length} ${visibleTeams.length === 1 ? "team" : "teams"}`;
  return (
    <>
      <PageHeader
        title="Teams"
        subtitle="All teams in the organisation"
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
              New team
            </Button>
          </>
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

        <TeamsTable teams={visibleTeams} />
      </PageBody>
    </>
  );
};

export default TeamsPage;
