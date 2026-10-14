import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router";
import { CssBaseline, ThemeProvider, createTheme } from "@mui/material";
import { SnackbarProvider } from "notistack";
import { CurrentUserProvider } from "./context/CurrentUserContext";

import Toast from "./components/molecules/Toast/Toast";
import "./index.css";
import App from "./App.jsx";

createRoot(document.getElementById("root")).render(
  <StrictMode>
    <CssBaseline />
    <BrowserRouter>
      <SnackbarProvider
        maxSnack={1}
        autoHideDuration={5000}
        transitionDuration={{ enter: 0, exit: 0 }}
        anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
        Components={{ toast: Toast }}
      >
        <CurrentUserProvider>
          <App />
        </CurrentUserProvider>
      </SnackbarProvider>
    </BrowserRouter>
  </StrictMode>,
);
