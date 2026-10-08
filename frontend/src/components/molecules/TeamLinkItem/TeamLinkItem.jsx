import { Box, ButtonBase, Typography } from "@mui/material";
import { Link as RouterLink } from "react-router";
import { ChevronRight } from "lucide-react";

const TeamLinkItem = ({ name, members, projects, to }) => {
  return (
    <ButtonBase
      component={RouterLink}
      to={to}
      sx={{
        width: "100%",
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        p: "12px 20px",
        minHeight: "68px",
        textAlign: "left",
        borderBottom: "1px solid #efefef",
      }}
    >
      <Box>
        <Typography
          sx={{
            fontSize: "14px",
            fontWeight: 500,
            color: "#111827",
            lineHeight: "20px",
          }}
        >
          {name}
        </Typography>
        <Typography
          sx={{ fontSize: "13px", color: "#6b7280", lineHeight: "18px" }}
        >
          {members} members · {projects} projects
        </Typography>
      </Box>
      <ChevronRight size={16} color="#6b7280" />
    </ButtonBase>
  );
};

export default TeamLinkItem;
