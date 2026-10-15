import { Box, Typography } from "@mui/material";
import FieldLabel from "../../atoms/FieldLabel/FieldLabel";
import Tag from "../../atoms/Tag/Tag";

const TagGroup = ({
  label,
  items = [],
  tone = "neutral",
  emptyText = "none",
}) => {
  return (
    <Box>
      <FieldLabel>{label}</FieldLabel>
      {items.length === 0 ? (
        <Typography
          sx={{ fontSize: "14px", lineHeight: "20px", color: "#6b7280" }}
        >
          {emptyText}
        </Typography>
      ) : (
        <Box sx={{ display: "flex", flexWrap: "wrap", gap: "8px" }}>
          {items.map((item) => (
            <Tag key={item} tone={tone}>
              {item}
            </Tag>
          ))}
        </Box>
      )}
    </Box>
  );
};

export default TagGroup;
