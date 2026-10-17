import { Fragment } from "react";
import { Box, Stack, Typography } from "@mui/material";

const circleSx = (state) => ({
  width: "24px",
  height: "24px",
  flexShrink: 0,
  borderRadius: "50%",
  display: "flex",
  alignItems: "center",
  justifyContent: "center",
  fontSize: "12px",
  fontWeight: 500,
  ...(state === "done" && {
    backgroundColor: "#365aff",
    color: "#ffffff",
    border: "1px solid #365aff",
  }),
  ...(state === "current" && {
    backgroundColor: "#eff7ff",
    color: "#2305ca",
    border: "1px solid #365aff",
  }),
  ...(state === "upcoming" && {
    backgroundColor: "#ffffff",
    color: "#6b7280",
    border: "1px solid #e5e7eb",
  }),
});

const WizardSteps = ({ steps, current }) => (
  <Stack direction="row" sx={{ alignItems: "center", width: "100%" }}>
    {steps.map((label, index) => {
      const state =
        index < current ? "done" : index === current ? "current" : "upcoming";

      return (
        <Fragment key={label}>
          {index > 0 && (
            <Box
              sx={{
                flex: 1,
                minWidth: "24px",
                height: "1px",
                backgroundColor: "#e5e7eb",
                mx: "10px",
              }}
            />
          )}

          <Stack direction="row" sx={{ alignItems: "center", flexShrink: 0 }}>
            <Box sx={circleSx(state)}>{index + 1}</Box>
            <Typography
              sx={{
                ml: "10px",
                fontSize: "13px",
                lineHeight: "22px",
                whiteSpace: "nowrap",
                color: state === "current" ? "#111827" : "#6b7280",
              }}
            >
              {label}
            </Typography>
          </Stack>
        </Fragment>
      );
    })}
  </Stack>
);

export default WizardSteps;
