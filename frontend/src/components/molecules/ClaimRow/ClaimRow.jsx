import { Box, Stack, Typography } from "@mui/material";
import Tag from "../../atoms/Tag/Tag";
import Switch from "../../atoms/Switch/Switch";

const ClaimRow = ({
  name,
  description,
  feature,
  checked,
  disabled,
  onChange,
}) => {
  return (
    <Stack
      direction="row"
      sx={{
        alignItems: "center",
        justifyContent: "space-between",
        gap: "16px",
        p: "14px 24px",
        borderTop: "1px solid #e5e7eb",
      }}
    >
      <Box sx={{ minWidth: 0 }}>
        <Stack direction="row" sx={{ alignItems: "center", gap: "8px" }}>
          <Typography
            sx={{
              fontFamily:
                "ui-monospace, SFMono-Regular, Menlo, Consolas, monospace",
              fontSize: "13px",
              color: "#1e1b4b",
              lineHeight: "20px",
            }}
          >
            {name}
          </Typography>
          {feature && <Tag tone="info">{feature}</Tag>}
        </Stack>
        <Typography
          sx={{ fontSize: "14px", color: "#6b7280", lineHeight: "20px" }}
        >
          {description}
        </Typography>
      </Box>
      <Switch
        checked={checked}
        disabled={disabled}
        onChange={onChange}
        ariaLabel={name}
      />
    </Stack>
  );
};

export default ClaimRow;
