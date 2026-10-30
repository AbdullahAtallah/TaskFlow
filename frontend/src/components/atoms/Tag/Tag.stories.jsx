import Tag from "./Tag";

export default {
  title: "Atoms/Tag",
  component: Tag,
  args: { children: "Admin" },
};

export const Neutral = {};
export const Success = { args: { tone: "success", children: "Active" } };
export const Info = { args: { tone: "info", children: "Default" } };
export const Purple = { args: { tone: "purple", children: "In progress" } };
