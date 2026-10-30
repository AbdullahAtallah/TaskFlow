import { useState } from "react";
import PackageCard from "./PackageCard";
import { PERMISSION_CATALOG } from "../../../constants/permissionCatalog";

const byName = (name) => PERMISSION_CATALOG.find((p) => p.name === name);

export default {
  title: "Organisms/PackageCard",
  component: PackageCard,
};

const InteractivePackageCard = () => {
  const pkg = byName("Tasks");
  const [enabled, setEnabled] = useState(true);
  const [claims, setClaims] = useState(["Tasks.Create", "Tasks.Manage"]);
  return (
    <div style={{ maxWidth: 640 }}>
      <PackageCard
        pkg={pkg}
        enabled={enabled}
        claims={claims}
        onTogglePackage={(_, on) => {
          setEnabled(on);
          if (!on) setClaims([]);
        }}
        onToggleClaim={(name, on) =>
          setClaims((prev) =>
            on ? [...prev, name] : prev.filter((c) => c !== name),
          )
        }
      />
    </div>
  );
};

export const Interactive = { render: () => <InteractivePackageCard /> };
