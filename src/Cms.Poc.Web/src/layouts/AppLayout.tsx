import { useLayoutEffect } from "react";
import { Outlet, Link as RouterLink, useLocation } from "react-router-dom";
import { AppBar, Box, Button, MenuItem, Select, Stack, Toolbar } from "@mui/material";
import { useTranslation } from "react-i18next";
import { useLanguage } from "../hooks/useLanguage";
import { getStoredUiLanguage } from "../i18n/i18n";
import UserMenu from "../components/UserMenu";
import AppearanceMenu from "../components/AppearanceMenu";
import CmsEsLogo from "../components/CmsEsLogo";
import Footer from "../components/Footer";

export default function AppLayout() {
  const { t, i18n } = useTranslation();
  const { language, setLanguage, languages } = useLanguage();
  // The CMS shows every language at once, so the selector is for the public site only.
  const isCms = useLocation().pathname.startsWith("/cms");

  // The public site's static strings always follow its content language (falling back to
  // English where untranslated); the CMS uses the editor's own persisted UI language.
  // A layout effect so the switch lands before paint instead of flashing the old language.
  const uiLanguage = isCms ? getStoredUiLanguage() : language;
  useLayoutEffect(() => {
    if (i18n.language !== uiLanguage) void i18n.changeLanguage(uiLanguage);
    document.documentElement.lang = uiLanguage;
  }, [i18n, uiLanguage]);

  return (
    <Box sx={{ display: "flex", flexDirection: "column", minHeight: "100vh" }}>
      {/* position="relative" (rather than "static") gives the bar a stacking context so its
          drop shadow paints on top of the CMS nav drawer sitting below it, instead of being
          hidden behind the drawer's own opaque background. */}
      <AppBar position="relative" sx={{ zIndex: (theme) => theme.zIndex.appBar }}>
        <Toolbar sx={{ gap: 2 }}>
          <Box
            component={RouterLink}
            to={`/${language}`}
            sx={{ flexGrow: 1, display: "flex", alignItems: "center", color: "text.primary" }}
          >
            <CmsEsLogo height={22} />
          </Box>
          {isCms ? (
            <UserMenu />
          ) : (
            <>
              <Stack direction="row" spacing={1}>
                <Button component={RouterLink} to={`/${language}/news`} color="inherit" size="small">
                  {t("nav.news")}
                </Button>
                <Button component={RouterLink} to={`/${language}/events`} color="inherit" size="small">
                  {t("nav.events")}
                </Button>
              </Stack>
              <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
                <AppearanceMenu />
                <Select
                  value={language}
                  onChange={(e) => setLanguage(e.target.value)}
                  size="small"
                  sx={{ width: 100 }}
                  inputProps={{ "aria-label": t("language.switchLabel") }}
                >
                  {languages.map((l) => (
                    <MenuItem key={l} value={l}>
                      {l}
                    </MenuItem>
                  ))}
                </Select>
              </Stack>
            </>
          )}
        </Toolbar>
      </AppBar>
      <Box sx={{ flex: 1, display: "flex" }}>
        <Outlet />
      </Box>
      {!isCms && <Footer />}
    </Box>
  );
}
