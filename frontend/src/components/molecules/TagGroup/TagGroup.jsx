import { Box } from "@mui/material";
import FieldLabel from "../../atoms/FieldLabel/FieldLabel";
import Tag from "../../atoms/Tag/Tag";

const TagGroup = ({ label, items, tone = "neutral" }) => {
  return (
    <Box>
      <FieldLabel>{label}</FieldLabel>
      <Box sx={{ display: "flex", flexWrap: "wrap", gap: "8px" }}>
        {items.map((item) => (
          <Tag key={item} tone={tone}>
            {item}
          </Tag>
        ))}
      </Box>
    </Box>
  );
};

export default TagGroup;
