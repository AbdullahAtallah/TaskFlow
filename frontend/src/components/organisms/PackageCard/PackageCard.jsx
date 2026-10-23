import { Box, Stack, Typography } from "@mui/material";
import Tag from "../../atoms/Tag/Tag";
import Switch from "../../atoms/Switch/Switch";
import ClaimRow from "../../molecules/ClaimRow/ClaimRow";

const PackageCard = ({
  pkg,
  enabled,
  locked,
  lockedNote,
  claims,
  onTogglePackage,
  onToggleClaim,
}) => {
  return (
    <Box
      sx={{
        border: "1px solid #e5e7eb",
        borderRadius: "8px",
        backgroundColor: "#ffffff",
        overflow: "hidden",
      }}
    >
      <Stack
        direction="row"
        sx={{
          alignItems: "center",
          justifyContent: "space-between",
          gap: "16px",
          p: "16px 20px",
          backgroundColor: "#f8f9fb",
        }}
      >
        <Box>
          <Typography
            sx={{
              fontSize: "16px",
              fontWeight: 600,
              color: "#111827",
              lineHeight: "24px",
            }}
          >
            {pkg.name}
          </Typography>
          <Typography
            sx={{ fontSize: "14px", color: "#6b7280", lineHeight: "20px" }}
          >
            {pkg.description}
          </Typography>
          {lockedNote && (
            <Typography
              sx={{
                mt: "4px",
                fontSize: "13px",
                color: "#9ca3af",
                lineHeight: "18px",
              }}
            >
              {lockedNote}
            </Typography>
          )}
        </Box>
        {pkg.alwaysOn ? (
          <Tag>Always on</Tag>
        ) : (
          <Switch
            checked={enabled}
            disabled={locked}
            onChange={(on) => onTogglePackage(pkg, on)}
            ariaLabel={pkg.name}
          />
        )}
      </Stack>

      {pkg.claims.length === 0 ? (
        <Typography
          sx={{
            p: "14px 24px",
            fontSize: "14px",
            color: "#6b7280",
            borderTop: "1px solid #e5e7eb",
          }}
        >
          Package only — no separate claims.
        </Typography>
      ) : (
        pkg.claims.map((claim) => (
          <ClaimRow
            key={claim.name}
            name={claim.name}
            description={claim.description}
            feature={claim.feature}
            checked={claims.includes(claim.name)}
            disabled={!enabled}
            onChange={(on) => onToggleClaim(claim.name, on)}
          />
        ))
      )}
    </Box>
  );
};

export default PackageCard;
