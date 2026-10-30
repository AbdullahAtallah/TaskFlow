import AppButton from "./AppButton";

export default {
  title: "Atoms/AppButton",
  component: AppButton,
  args: { children: "Save" },
  argTypes: {
    variant: {
      control: "inline-radio",
      options: ["primary", "secondary", "danger"],
    },
  },
};

export const Primary = {};
export const Secondary = { args: { variant: "secondary", children: "Cancel" } };
export const Danger = { args: { variant: "danger", children: "Delete team" } };
export const Disabled = { args: { disabled: true } };
