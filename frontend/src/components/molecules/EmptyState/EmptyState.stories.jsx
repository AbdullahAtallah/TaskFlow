import EmptyState from "./EmptyState";

export default {
  title: "Molecules/EmptyState",
  component: EmptyState,
  args: { title: "No teams found", text: 'Nothing matches "zzz".' },
  argTypes: {
    title: { control: "text" },
    text: { control: "text" },
  },
};

export const Default = {};
export const TitleOnly = { args: { text: undefined } };
