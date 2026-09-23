import { createContext, useContext, useMemo, useState, type ReactNode } from "react";
import { CssBaseline, ThemeProvider, createTheme } from "@mui/material";

type ThemeMode = "light" | "dark";

const STORAGE_KEY = "cms-poc-theme-mode";
const FROSTED_GLASS_STORAGE_KEY = "cms-poc-frosted-glass";
const GLASS_OPACITY_STORAGE_KEY = "cms-poc-frosted-glass-opacity";
const GLASS_OPACITY_DEFAULT = 0.8;
// Capped below 1 so the slider can never select a fully opaque ("not glass at all")
// surface - turning frosted glass fully off is what the separate on/off toggle is for.
const GLASS_OPACITY_MIN = 0.3;
const GLASS_OPACITY_MAX = 0.95;

function clampGlassOpacity(value: number): number {
  return Math.min(GLASS_OPACITY_MAX, Math.max(GLASS_OPACITY_MIN, value));
}

const GLASS_BLUR_STORAGE_KEY = "cms-poc-frosted-glass-blur";
const GLASS_BLUR_DEFAULT = 16;
const GLASS_BLUR_MIN = 0;
const GLASS_BLUR_MAX = 32;

function clampGlassBlur(value: number): number {
  return Math.min(GLASS_BLUR_MAX, Math.max(GLASS_BLUR_MIN, value));
}

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
export function glassPanel(mode: ThemeMode, opacity: number, blurPx: number): { backgroundColor: string; backdropFilter: string; WebkitBackdropFilter: string } {
  const backdropFilter = `blur(${blurPx}px) saturate(180%)`;
  const rgb = mode === "light" ? "255,255,255" : "37,37,38";
  return {
    backgroundColor: `rgba(${rgb},${opacity})`,
    backdropFilter,
    WebkitBackdropFilter: backdropFilter,
  };
}

function getInitialMode(): ThemeMode {
  const stored = localStorage.getItem(STORAGE_KEY);
  if (stored === "light" || stored === "dark") return stored;
  return "dark";
}

function getInitialFrostedGlass(): boolean {
  const stored = localStorage.getItem(FROSTED_GLASS_STORAGE_KEY);
  if (stored === "on" || stored === "off") return stored === "on";
  return false;
}

function getInitialGlassOpacity(): number {
  const stored = Number(localStorage.getItem(GLASS_OPACITY_STORAGE_KEY));
  return Number.isFinite(stored) && stored > 0 ? clampGlassOpacity(stored) : GLASS_OPACITY_DEFAULT;
}

function getInitialGlassBlur(): number {
  const stored = Number(localStorage.getItem(GLASS_BLUR_STORAGE_KEY));
  return Number.isFinite(stored) && localStorage.getItem(GLASS_BLUR_STORAGE_KEY) !== null
    ? clampGlassBlur(stored)
    : GLASS_BLUR_DEFAULT;
}

interface ThemeModeContextValue {
  mode: ThemeMode;
  toggleMode: () => void;
  frostedGlass: boolean;
  toggleFrostedGlass: () => void;
  glassOpacity: number;
  setGlassOpacity: (opacity: number) => void;
  glassOpacityRange: { min: number; max: number };
  glassBlur: number;
  setGlassBlur: (blurPx: number) => void;
  glassBlurRange: { min: number; max: number };
}

const ThemeModeContext = createContext<ThemeModeContextValue | undefined>(undefined);

export function useThemeMode() {
  const context = useContext(ThemeModeContext);
  if (!context) throw new Error("useThemeMode must be used within a ThemeModeProvider");
  return context;
}

export function ThemeModeProvider({ children }: { children: ReactNode }) {
  const [mode, setMode] = useState<ThemeMode>(getInitialMode);
  const [frostedGlass, setFrostedGlass] = useState<boolean>(getInitialFrostedGlass);
  const [glassOpacity, setGlassOpacityState] = useState<number>(getInitialGlassOpacity);
  const [glassBlur, setGlassBlurState] = useState<number>(getInitialGlassBlur);

  const toggleMode = () => {
    setMode((prev) => {
      const next = prev === "light" ? "dark" : "light";
      localStorage.setItem(STORAGE_KEY, next);
      return next;
    });
  };

  const toggleFrostedGlass = () => {
    setFrostedGlass((prev) => {
      const next = !prev;
      localStorage.setItem(FROSTED_GLASS_STORAGE_KEY, next ? "on" : "off");
      return next;
    });
  };

  const setGlassOpacity = (opacity: number) => {
    const clamped = clampGlassOpacity(opacity);
    localStorage.setItem(GLASS_OPACITY_STORAGE_KEY, String(clamped));
    setGlassOpacityState(clamped);
  };

  const setGlassBlur = (blurPx: number) => {
    const clamped = clampGlassBlur(blurPx);
    localStorage.setItem(GLASS_BLUR_STORAGE_KEY, String(clamped));
    setGlassBlurState(clamped);
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
            // index.css sets `color-scheme: light dark` as a pre-hydration default, which
            // leaves native chrome (scrollbars, form controls) following the OS preference
            // instead of the app's own light/dark toggle. Pin it to the actual mode here.
            html: { colorScheme: mode },
            // The gradient's radial-gradient() positions are baked into the string itself,
            // so `background-position` can't animate them. Instead the gradient lives on a
            // fixed ::before layer, scaled up slightly so its edges stay off-screen, and the
            // whole layer is nudged with `transform` - that's what makes the drift visible.
            "@keyframes frostedGlassDrift": {
              "0%": { transform: "scale(1.08) translate3d(0%, 0%, 0)" },
              "50%": { transform: "scale(1.08) translate3d(-3%, 3%, 0)" },
              "100%": { transform: "scale(1.08) translate3d(0%, 0%, 0)" },
            },
            body: frostedGlass
              ? {
                  backgroundColor: backgroundDefault,
                  position: "relative",
                  "&::before": {
                    content: '""',
                    position: "fixed",
                    inset: 0,
                    zIndex: -1,
                    backgroundImage: backgroundGradient,
                    backgroundRepeat: "no-repeat",
                    transformOrigin: "center center",
                    animation: "frostedGlassDrift 24s ease-in-out infinite",
                    "@media (prefers-reduced-motion: reduce)": {
                      animation: "none",
                    },
                  },
                }
              : { backgroundColor: backgroundDefault },
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
          // on its root once here rather than being repeated per-component. With frosted
          // glass off, Paper falls back to its normal palette-driven solid background.
          styleOverrides: {
            root: { backgroundImage: "none", border: "none", ...(frostedGlass ? glassPanel(mode, glassOpacity, glassBlur) : {}) },
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
            // color="transparent" makes MUI apply its own background-color: transparent
            // class, which otherwise wins over MuiPaper's root background and leaves the
            // AppBar not matching the Drawer (also a Paper) right below it. Setting it
            // explicitly here instead of relying on inheritance fixes that.
            root: {
              boxShadow: chromeShadow(mode, "down"),
              backgroundImage: "none",
              ...(frostedGlass ? glassPanel(mode, glassOpacity, glassBlur) : { backgroundColor: backgroundPaper }),
            },
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
  }, [mode, frostedGlass, glassOpacity, glassBlur]);

  return (
    <ThemeModeContext.Provider
      value={{
        mode,
        toggleMode,
        frostedGlass,
        toggleFrostedGlass,
        glassOpacity,
        setGlassOpacity,
        glassOpacityRange: { min: GLASS_OPACITY_MIN, max: GLASS_OPACITY_MAX },
        glassBlur,
        setGlassBlur,
        glassBlurRange: { min: GLASS_BLUR_MIN, max: GLASS_BLUR_MAX },
      }}
    >
      <ThemeProvider theme={theme}>
        <CssBaseline />
        {children}
      </ThemeProvider>
    </ThemeModeContext.Provider>
  );
}
