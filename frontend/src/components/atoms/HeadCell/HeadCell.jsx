import { TableCell } from "@mui/material";

const HeadCell = ({ children, width }) => {
  return (
    <TableCell
      sx={{
        width,
        height: "44px",
        px: "16px",
        py: 0,
        textAlign: "left",
        whiteSpace: "nowrap",
        backgroundColor: "#fdfdfd",
        borderBottom: "1px solid #e5e7eb",
        fontFamily: "inherit",
        fontWeight: 500,
        fontSize: "13px",
        lineHeight: "100%",
        color: "#313131",
        boxSizing: "border-box",
      }}
    >
      {children}
    </TableCell>
  );
};

export default HeadCell;
