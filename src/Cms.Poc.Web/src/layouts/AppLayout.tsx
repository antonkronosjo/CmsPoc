import { Outlet, Link as RouterLink } from "react-router-dom";
import { AppBar, Box, IconButton, MenuItem, Select, Toolbar, Typography } from "@mui/material";
import { DarkMode, LightMode } from "@mui/icons-material";
import { useThemeMode } from "../theme/ThemeModeProvider";
import { LANGUAGES, useLanguage } from "../context/LanguageContext";

export default function AppLayout() {
  const { mode, toggleMode } = useThemeMode();
  const { language, setLanguage } = useLanguage();

  return (
    <Box sx={{ display: "flex", flexDirection: "column", minHeight: "100vh" }}>
      <AppBar position="static" color="default" elevation={1}>
        <Toolbar sx={{ gap: 2 }}>
          <Typography
            variant="h6"
            component={RouterLink}
            to="/"
            sx={{ flexGrow: 1, color: "inherit", textDecoration: "none" }}
          >
            Content Framework POC
          </Typography>
          <Select value={language} onChange={(e) => setLanguage(e.target.value)} size="small" sx={{ width: 100 }}>
            {LANGUAGES.map((l) => (
              <MenuItem key={l} value={l}>
                {l}
              </MenuItem>
            ))}
          </Select>
          <IconButton onClick={toggleMode} aria-label="Toggle dark mode">
            {mode === "dark" ? <LightMode /> : <DarkMode />}
          </IconButton>
        </Toolbar>
      </AppBar>
      <Box sx={{ flex: 1, display: "flex" }}>
        <Outlet />
      </Box>
    </Box>
  );
}
