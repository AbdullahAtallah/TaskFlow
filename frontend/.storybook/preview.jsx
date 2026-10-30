import { MemoryRouter } from "react-router";
import { CssBaseline } from "@mui/material";

const preview = {
  decorators: [
    (Story) => (
      <MemoryRouter>
        <CssBaseline />
        <div
          style={{ padding: 24, fontFamily: "Inter, system-ui, sans-serif" }}
        >
          <Story />
        </div>
      </MemoryRouter>
    ),
  ],
  parameters: {
    layout: "fullscreen",
  },
};

export default preview;
