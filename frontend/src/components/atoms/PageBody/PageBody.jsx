import { Box } from "@mui/material";

const PageBody = ({ children }) => {
  return (
    <Box sx={{ p: { xs: "16px 16px 32px", md: "24px 32px 48px" } }}>
      {children}
    </Box>
  );
};

export default PageBody;
