import { Box } from "@mui/material";
import PageTitle from "../../atoms/PageTitle/PageTitle";
import PageSubtitle from "../../atoms/PageSubtitle/PageSubtitle";

const PageHeader = ({ title, subtitle, actions }) => {
  return (
    <Box
      sx={{
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        gap: "16px",
        px: "36px",
        py: "28px",
        backgroundColor: "#f8f8fa",
        borderBottom: "1px solid #e5e7eb",
      }}
    >
      <Box>
        <PageTitle>{title}</PageTitle>
        {subtitle && <PageSubtitle>{subtitle}</PageSubtitle>}
      </Box>
      {actions && <Box sx={{ display: "flex", gap: "12px" }}>{actions}</Box>}
    </Box>
  );
};

export default PageHeader;
