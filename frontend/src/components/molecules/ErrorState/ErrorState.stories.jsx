import ErrorState from "./ErrorState";

export default {
  title: "Molecules/ErrorState",
  component: ErrorState,
  args: { message: "The teams could not be loaded.", onRetry: () => {} },
  argTypes: {
    title: { control: "text" },
    message: { control: "text" },
  },
};

export const WithRetry = {};
export const WithoutRetry = { args: { onRetry: undefined } };
export const ServerDown = {
  args: {
    title: "Can't reach the server",
    message:
      "TaskFlow could not connect. Check that the API is running, then try again.",
  },
};
