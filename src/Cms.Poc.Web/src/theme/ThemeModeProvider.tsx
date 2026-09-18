import { createContext, useContext, useMemo, useState, type ReactNode } from "react";
import { CssBaseline, ThemeProvider, createTheme } from "@mui/material";

type ThemeMode = "light" | "dark";

const STORAGE_KEY = "cms-poc-theme-mode";

function getInitialMode(): ThemeMode {
  const stored = localStorage.getItem(STORAGE_KEY);
  if (stored === "light" || stored === "dark") return stored;
  return "dark";
}

interface ThemeModeContextValue {
  mode: ThemeMode;
  toggleMode: () => void;
}

const ThemeModeContext = createContext<ThemeModeContextValue | undefined>(undefined);

export function useThemeMode() {
  const context = useContext(ThemeModeContext);
  if (!context) throw new Error("useThemeMode must be used within a ThemeModeProvider");
  return context;
}

export function ThemeModeProvider({ children }: { children: ReactNode }) {
  const [mode, setMode] = useState<ThemeMode>(getInitialMode);

  const toggleMode = () => {
    setMode((prev) => {
      const next = prev === "light" ? "dark" : "light";
      localStorage.setItem(STORAGE_KEY, next);
      return next;
    });
  };

  const theme = useMemo(() => {
    const isLight = mode === "light";
    const cardShadow = isLight
      ? "0px 2px 8px rgba(16,24,40,0.06), 0px 1px 2px rgba(16,24,40,0.04)"
      : "0px 2px 8px rgba(0,0,0,0.4), 0px 1px 2px rgba(0,0,0,0.3)";
    const cellBorder = isLight ? "1px solid rgba(16,24,40,0.06)" : "1px solid rgba(255,255,255,0.06)";
    const backgroundDefault = isLight ? "#F6F7F9" : "#1E1E1E";
    const backgroundPaper = isLight ? "#FFFFFF" : "#252526";

    return createTheme({
      palette: {
        mode,
        background: { default: backgroundDefault, paper: backgroundPaper },
        primary: { main: isLight ? "#5B6EF5" : "#007ACC" },
        divider: isLight ? "rgba(16,24,40,0.06)" : "rgba(255,255,255,0.09)",
        ...(isLight
          ? {}
          : {
              text: { primary: "#D4D4D4", secondary: "#9D9D9D" },
            }),
      },
      shape: { borderRadius: 0 },
      typography: {
        fontFamily: `"Inter", "Roboto", "Helvetica", "Arial", sans-serif`,
        fontSize: 14,
        h4: { fontSize: "1.75rem", fontWeight: 700, letterSpacing: "-0.02em" },
        h5: { fontSize: "1.375rem", fontWeight: 700, letterSpacing: "-0.01em" },
        h6: { fontSize: "1.0625rem", fontWeight: 600 },
        subtitle2: { fontSize: "0.8125rem", fontWeight: 500 },
        body1: { fontSize: "0.9375rem" },
        body2: { fontSize: "0.8125rem" },
        button: { fontWeight: 600 },
      },
      components: {
        MuiSvgIcon: {
          defaultProps: { fontSize: "small" },
        },
        MuiCssBaseline: {
          styleOverrides: { body: { backgroundColor: backgroundDefault } },
        },
        MuiPaper: {
          styleOverrides: {
            root: { backgroundImage: "none", border: "none" },
            elevation1: { boxShadow: cardShadow },
          },
        },
        MuiCard: {
          defaultProps: { elevation: 1 },
          styleOverrides: {
            root: { borderRadius: 0, border: "none", boxShadow: cardShadow },
          },
        },
        MuiAppBar: {
          defaultProps: { elevation: 0, color: "transparent" },
          styleOverrides: {
            root: { boxShadow: "none", backgroundColor: backgroundPaper, backgroundImage: "none" },
          },
        },
        MuiDrawer: {
          styleOverrides: {
            paper: { border: "none" },
          },
        },
        MuiTableCell: {
          styleOverrides: {
            root: { borderBottom: cellBorder, padding: "12px 16px" },
            head: { fontWeight: 600, color: isLight ? "#6B7280" : "#9CA3AF" },
          },
        },
        MuiTableRow: {
          styleOverrides: {
            root: { "&:last-child .MuiTableCell-root": { borderBottom: "none" } },
          },
        },
        MuiListItemButton: {
          styleOverrides: {
            root: { borderRadius: 0, marginBottom: 4 },
          },
        },
        MuiTextField: {
          defaultProps: { variant: "outlined" },
        },
        MuiOutlinedInput: {
          styleOverrides: {
            root: {
              "&.Mui-focused .MuiOutlinedInput-notchedOutline": { borderWidth: 1 },
            },
          },
        },
        MuiButton: {
          defaultProps: { disableElevation: true },
          styleOverrides: {
            root: { borderRadius: 0, textTransform: "none" },
          },
        },
        MuiChip: {
          styleOverrides: { root: { borderRadius: 0 } },
        },
      },
    });
  }, [mode]);

  return (
    <ThemeModeContext.Provider value={{ mode, toggleMode }}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        {children}
      </ThemeProvider>
    </ThemeModeContext.Provider>
  );
}
