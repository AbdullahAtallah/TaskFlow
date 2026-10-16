import { Tab, Tabs } from "@mui/material";
import NavCount from "../../atoms/NavCount/NavCount";

const PageTabs = ({ value, onChange, tabs }) => (
  <Tabs
    value={value}
    onChange={(_, v) => onChange(v)}
    sx={{
      minHeight: "46px",
      borderBottom: "1px solid #e5e7eb",
      "& .MuiTabs-indicator": {
        backgroundColor: "#365aff",
        height: "1px",
        transition: "none",
      },
    }}
  >
    {tabs.map((tab) => (
      <Tab
        key={tab.value}
        value={tab.value}
        disableRipple
        label={
          <span
            style={{ display: "inline-flex", alignItems: "center", gap: "8px" }}
          >
            {tab.label}
            {tab.count !== undefined && <NavCount>{tab.count}</NavCount>}
          </span>
        }
        sx={{
          textTransform: "none",
          minHeight: "46px",
          px: "6px",
          mr: "24px",
          minWidth: 0,
          fontSize: "14px",
          fontWeight: 400,
          color: "#6b7280",
          "&.Mui-selected": { color: "#111827" },
        }}
      />
    ))}
  </Tabs>
);

export default PageTabs;
