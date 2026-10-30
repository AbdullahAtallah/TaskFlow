import { useState } from "react";
import Switch from "./Switch";

export default {
  title: "Atoms/Switch",
  component: Switch,
};

const InteractiveSwitch = () => {
  const [on, setOn] = useState(true);
  return <Switch checked={on} onChange={setOn} ariaLabel="Demo" />;
};

export const Interactive = { render: () => <InteractiveSwitch /> };
export const On = { args: { checked: true, ariaLabel: "On" } };
export const Off = { args: { checked: false, ariaLabel: "Off" } };
export const Disabled = {
  args: { checked: true, disabled: true, ariaLabel: "Disabled" },
};
