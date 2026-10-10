import { ChevronsUpDown } from "lucide-react";

const SortIcon = ({ active = false }) => {
  return (
    <ChevronsUpDown
      size={18}
      strokeWidth={2}
      color={active ? "#313131" : "#b6b4b4"}
    />
  );
};

export default SortIcon;
