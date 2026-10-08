import LinearProgress, {
  linearProgressClasses,
} from "@mui/material/LinearProgress";

const ProgressBar = ({ value, max }) => {
  const percent = max > 0 ? (value / max) * 100 : 0;

  return (
    <LinearProgress
      variant="determinate"
      value={percent}
      sx={{
        width: "100%",
        height: "8px",
        borderRadius: "999px",
        backgroundColor: "#f3f4f6",
        [`& .${linearProgressClasses.bar}`]: {
          borderRadius: "999px",
          backgroundColor: "#365aff",
        },
      }}
    />
  );
};

export default ProgressBar;
