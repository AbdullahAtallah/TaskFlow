import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router";
import { CssBaseline, ThemeProvider, createTheme } from "@mui/material";
import { SnackbarProvider } from "notistack";
import Toast from "./components/molecules/Toast/Toast";
import "./index.css";
import App from "./App.jsx";

const theme = createTheme({
  typography: { fontFamily: "'Inter', system-ui, sans-serif" },
});

createRoot(document.getElementById("root")).render(
  <StrictMode>
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <BrowserRouter>
        <SnackbarProvider
          maxSnack={1}
          autoHideDuration={5000}
          transitionDuration={{ enter: 0, exit: 0 }}
          anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
          Components={{ toast: Toast }}
        >
          <App />
        </SnackbarProvider>
      </BrowserRouter>
    </ThemeProvider>
  </StrictMode>,
);
