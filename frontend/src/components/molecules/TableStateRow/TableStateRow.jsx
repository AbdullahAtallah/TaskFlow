import { TableCell, TableRow } from "@mui/material";
import EmptyState from "../EmptyState/EmptyState";
import LoadingState from "../LoadingState/LoadingState";

const TableStateRow = ({ colSpan, loading, emptyTitle, emptyText }) => (
  <TableRow>
    <TableCell colSpan={colSpan} sx={{ p: 0, borderBottom: 0 }}>
      {loading ? (
        <LoadingState />
      ) : (
        <EmptyState title={emptyTitle} text={emptyText} />
      )}
    </TableCell>
  </TableRow>
);

export default TableStateRow;
