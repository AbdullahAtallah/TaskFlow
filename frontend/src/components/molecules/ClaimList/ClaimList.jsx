import { Box } from "@mui/material";
import FieldLabel from "../../atoms/FieldLabel/FieldLabel";
import ClaimText from "../../atoms/ClaimText/ClaimText";

const ClaimList = ({ label = "Claims", claims }) => {
  return (
    <Box sx={{ display: "flex", flexDirection: "column" }}>
      <FieldLabel>{label}</FieldLabel>
      {claims.map((claim) => (
        <ClaimText key={claim}>{claim}</ClaimText>
      ))}
    </Box>
  );
};

export default ClaimList;
