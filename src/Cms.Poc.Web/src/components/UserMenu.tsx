import { useState, type MouseEvent } from "react";
import { Avatar, Box, ButtonBase, ListItemIcon, ListItemText, Menu, MenuItem, Slider, Switch, Typography } from "@mui/material";
import { AccountCircle, BlurOff, BlurOn, DarkMode, LightMode, Translate } from "@mui/icons-material";
import { useTranslation } from "react-i18next";
import { useUser } from "../context/UserContext";
import { useThemeMode } from "../theme/ThemeModeProvider";
import { useUiLanguage } from "../hooks/useUiLanguage";

/// Avatar (plus display name) in the CMS header that opens a settings menu. The switches and
/// the opacity slider only call their own setter - they never close the menu - so any of
/// them can be adjusted back and forth without reopening the menu each time.
export default function UserMenu() {
  const { t } = useTranslation();
  const { displayName } = useUser();
  const {
    mode,
    toggleMode,
    frostedGlass,
    toggleFrostedGlass,
    glassOpacity,
    setGlassOpacity,
    glassOpacityRange,
    glassBlur,
    setGlassBlur,
    glassBlurRange,
  } = useThemeMode();
  const { uiLanguage, setUiLanguage } = useUiLanguage();
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);

  return (
    <>
      <ButtonBase
        onClick={(e: MouseEvent<HTMLElement>) => setAnchorEl(e.currentTarget)}
        aria-label={t("nav.userSettings")}
        sx={{ display: "flex", alignItems: "center", gap: 1, borderRadius: 999, px: 0.5, py: 0.25 }}
      >
        <Avatar sx={{ width: 32, height: 32 }}>
          <AccountCircle />
        </Avatar>
        {displayName && (
          <Typography variant="body2" sx={{ fontWeight: 600, color: "inherit" }}>
            {displayName}
          </Typography>
        )}
      </ButtonBase>
      <Menu
        anchorEl={anchorEl}
        open={!!anchorEl}
        onClose={() => setAnchorEl(null)}
        anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
        transformOrigin={{ vertical: "top", horizontal: "right" }}
      >
        <MenuItem disableRipple onClick={(e) => e.stopPropagation()} sx={{ "&:hover": { bgcolor: "transparent" } }}>
          <ListItemIcon>{mode === "dark" ? <DarkMode fontSize="small" /> : <LightMode fontSize="small" />}</ListItemIcon>
          <ListItemText>{mode === "dark" ? t("theme.darkMode") : t("theme.lightMode")}</ListItemText>
          <Switch checked={mode === "dark"} onChange={toggleMode} edge="end" size="small" sx={{ ml: 2 }} />
        </MenuItem>
        <MenuItem disableRipple onClick={(e) => e.stopPropagation()} sx={{ "&:hover": { bgcolor: "transparent" } }}>
          <ListItemIcon>
            <Translate fontSize="small" />
          </ListItemIcon>
          {/* Language names are shown as endonyms (their own name in their own language), not translated. */}
          <ListItemText>{uiLanguage === "sv" ? "Svenska" : "English"}</ListItemText>
          <Switch
            checked={uiLanguage === "sv"}
            onChange={(e) => setUiLanguage(e.target.checked ? "sv" : "en")}
            edge="end"
            size="small"
            sx={{ ml: 2 }}
          />
        </MenuItem>
        <MenuItem disableRipple onClick={(e) => e.stopPropagation()} sx={{ "&:hover": { bgcolor: "transparent" } }}>
          <ListItemIcon>{frostedGlass ? <BlurOn fontSize="small" /> : <BlurOff fontSize="small" />}</ListItemIcon>
          <ListItemText>{t("theme.frostedGlass")}</ListItemText>
          <Switch checked={frostedGlass} onChange={toggleFrostedGlass} edge="end" size="small" sx={{ ml: 2 }} />
        </MenuItem>
        <Box sx={{ px: 2, pt: 0.5, pb: 1.5, opacity: frostedGlass ? 1 : 0.4 }}>
          <Typography variant="caption" color="text.secondary">
            {t("theme.opacity", { percent: Math.round(glassOpacity * 100) })}
          </Typography>
          <Slider
            value={glassOpacity}
            onChange={(_, value) => setGlassOpacity(value as number)}
            min={glassOpacityRange.min}
            max={glassOpacityRange.max}
            step={0.01}
            disabled={!frostedGlass}
            size="small"
            sx={{ display: "block" }}
          />
        </Box>
        <Box sx={{ px: 2, pt: 0.5, pb: 1.5, opacity: frostedGlass ? 1 : 0.4 }}>
          <Typography variant="caption" color="text.secondary">
            {t("theme.blur", { px: glassBlur })}
          </Typography>
          <Slider
            value={glassBlur}
            onChange={(_, value) => setGlassBlur(value as number)}
            min={glassBlurRange.min}
            max={glassBlurRange.max}
            step={1}
            disabled={!frostedGlass}
            size="small"
            sx={{ display: "block" }}
          />
        </Box>
      </Menu>
    </>
  );
}
