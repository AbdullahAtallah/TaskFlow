import { TableCell } from "@mui/material";

const BodyCell = ({ children, bold = false }) => {
  return (
    <TableCell
      sx={{
        height: "60px",
        px: "16px",
        py: 0,
        position: "relative",
        verticalAlign: "middle",
        backgroundColor: "#ffffff",
        borderBottom: "1px solid #efefef",
        fontFamily: "inherit",
        fontWeight: bold ? 500 : 400,
        fontSize: "14px",
        lineHeight: "20px",
        color: bold ? "#111827" : "#313131",
        boxSizing: "border-box",
      }}
    >
      {children}
    </TableCell>
  );
};

export default BodyCell;
