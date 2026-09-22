import { Outlet, Link as RouterLink, useLocation } from "react-router-dom";
import { AppBar, Box, IconButton, MenuItem, Select, Toolbar, Typography } from "@mui/material";
import { DarkMode, LightMode } from "@mui/icons-material";
import { useThemeMode } from "../theme/ThemeModeProvider";
import { useLanguage } from "../hooks/useLanguage";
import UserMenu from "../components/UserMenu";

export default function AppLayout() {
  const { mode, toggleMode } = useThemeMode();
  const { language, setLanguage, languages } = useLanguage();
  // The CMS shows every language at once, so the selector is for the public site only.
  const isCms = useLocation().pathname.startsWith("/cms");

  return (
    <Box sx={{ display: "flex", flexDirection: "column", minHeight: "100vh" }}>
      {/* position="relative" (rather than "static") gives the bar a stacking context so its
          drop shadow paints on top of the CMS nav drawer sitting below it, instead of being
          hidden behind the drawer's own opaque background. */}
      <AppBar position="relative" sx={{ zIndex: (theme) => theme.zIndex.appBar }}>
        <Toolbar sx={{ gap: 2 }}>
          <Typography
            variant="h6"
            component={RouterLink}
            to={`/${language}`}
            sx={{ flexGrow: 1, color: "inherit", textDecoration: "none" }}
          >
            Content Framework POC
          </Typography>
          {!isCms && (
            <Select value={language} onChange={(e) => setLanguage(e.target.value)} size="small" sx={{ width: 100 }}>
              {languages.map((l) => (
                <MenuItem key={l} value={l}>
                  {l}
                </MenuItem>
              ))}
            </Select>
          )}
          {isCms ? (
            <UserMenu />
          ) : (
            <IconButton onClick={toggleMode} aria-label="Toggle dark mode">
              {mode === "dark" ? <LightMode /> : <DarkMode />}
            </IconButton>
          )}
        </Toolbar>
      </AppBar>
      <Box sx={{ flex: 1, display: "flex" }}>
        <Outlet />
      </Box>
    </Box>
  );
}
