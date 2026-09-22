import { createContext, useContext, useMemo, useState, type ReactNode } from "react";
import { CssBaseline, ThemeProvider, createTheme } from "@mui/material";

type ThemeMode = "light" | "dark";

const STORAGE_KEY = "cms-poc-theme-mode";

/// Shadow for fixed chrome (header, side nav) that should read as sitting above the
/// content, not a resting card - stronger and directional rather than the subtle,
/// even one on MuiCard/MuiPaper. Exported so CmsLayout's nav drawer (a separate file)
/// can match it instead of hardcoding its own values.
export function chromeShadow(mode: ThemeMode, direction: "down" | "right" = "down"): string {
  const offset = direction === "down" ? "0px 3px" : "3px 0px";
  return mode === "light" ? `${offset} 10px rgba(16,24,40,0.12)` : `${offset} 12px rgba(0,0,0,0.55)`;
}

/// Frosted-glass surface (translucent background + blur) for chrome and elevated panels
/// that sit above the app's gradient backdrop. Exported so CmsLayout's nav drawer (a
/// separate file) can match it instead of hardcoding its own values.
export function glassPanel(mode: ThemeMode): { backgroundColor: string; backdropFilter: string; WebkitBackdropFilter: string } {
  const backdropFilter = "blur(16px) saturate(180%)";
  return {
    backgroundColor: mode === "light" ? "rgba(255,255,255,0.72)" : "rgba(37,37,38,0.72)",
    backdropFilter,
    WebkitBackdropFilter: backdropFilter,
  };
}

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
    // Modeled on Apple-style wallpaper gradients: color pools that read as distinct
    // glassy discs rather than a diffuse wash - filled through most of their radius,
    // then a brighter rim right at the edge, then a hard cutoff to transparent, instead
    // of one long fade. Light mode keeps the same shape language but paler/airier;
    // dark mode goes darker/richer rather than a bright glow.
    const backgroundGradient = isLight
      ? "radial-gradient(circle at 75% -10%, rgba(91,110,245,0.10) 0%, rgba(91,110,245,0.14) 45%, rgba(150,165,255,0.22) 60%, rgba(150,165,255,0.22) 63%, transparent 66%), " +
        "radial-gradient(circle at 5% 100%, rgba(0,150,170,0.08) 0%, rgba(0,150,170,0.12) 45%, rgba(120,220,205,0.18) 60%, rgba(120,220,205,0.18) 63%, transparent 66%)"
      : "radial-gradient(circle at 75% -10%, rgba(15,35,120,0.5) 0%, rgba(20,45,150,0.55) 45%, rgba(110,150,255,0.65) 60%, rgba(110,150,255,0.65) 63%, transparent 66%), " +
        "radial-gradient(circle at 5% 100%, rgba(10,60,55,0.45) 0%, rgba(15,80,70,0.5) 45%, rgba(90,220,190,0.55) 60%, rgba(90,220,190,0.55) 63%, transparent 66%)";

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
          styleOverrides: {
            body: {
              backgroundColor: backgroundDefault,
              backgroundImage: backgroundGradient,
              backgroundAttachment: "fixed",
              backgroundRepeat: "no-repeat",
            },
            // The custom sharp-corners theme mutes MUI's default focus styling, so make
            // keyboard focus explicit rather than relying on browser defaults.
            "*:focus-visible": {
              outline: `2px solid ${isLight ? "#5B6EF5" : "#007ACC"}`,
              outlineOffset: 2,
            },
          },
        },
        MuiPaper: {
          // Paper underlies nearly every elevated surface (Card, AppBar, Drawer, Menu,
          // Popover, Dialog, Select/Autocomplete dropdowns), so the glass treatment goes
          // on its root once here rather than being repeated per-component.
          styleOverrides: {
            root: { backgroundImage: "none", border: "none", ...glassPanel(mode) },
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
            root: { boxShadow: chromeShadow(mode, "down"), backgroundImage: "none" },
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
