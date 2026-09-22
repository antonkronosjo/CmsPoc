import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { RouterProvider } from "react-router-dom";
import "./index.css";
import { router } from "./routes";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { ThemeModeProvider } from "./theme/ThemeModeProvider";
import { UserProvider } from "./context/UserContext";
import { ToastProvider } from "./context/ToastContext";
import ErrorBoundary from "./components/ErrorBoundary";

const queryClient = new QueryClient();

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <ThemeModeProvider>
          <ToastProvider>
            <LocalizationProvider dateAdapter={AdapterDayjs}>
              <UserProvider>
                <RouterProvider router={router} />
              </UserProvider>
            </LocalizationProvider>
          </ToastProvider>
        </ThemeModeProvider>
      </QueryClientProvider>
    </ErrorBoundary>
  </StrictMode>,
);
