import { useState } from "react";
import SegmentedTabs from "./SegmentedTabs";

export default {
  title: "Molecules/SegmentedTabs",
  component: SegmentedTabs,
};

const StatusTabs = () => {
  const [value, setValue] = useState("All");
  return (
    <SegmentedTabs
      options={["All", "To do", "In progress", "Done"]}
      value={value}
      onChange={setValue}
      ariaLabel="Status"
    />
  );
};

export const Status = { render: () => <StatusTabs /> };
