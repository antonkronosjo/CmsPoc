import type { ReactNode } from "react";
import { Box, ListItemIcon, ListItemText, MenuItem, Slider, Switch, Typography } from "@mui/material";
import { BlurOff, BlurOn, DarkMode, LightMode } from "@mui/icons-material";
import { useTranslation } from "react-i18next";
import { useThemeMode } from "../theme/ThemeModeProvider";

/// The theme controls (dark mode, frosted glass on/off, opacity, blur) shared by the CMS
/// user menu and the public site's appearance menu. Returned as a keyed array rather than a
/// component because MUI's Menu needs its items as direct children for keyboard navigation.
/// The switches and sliders only call their own setter - they never close the menu - so any
/// of them can be adjusted back and forth without reopening it.
export function useAppearanceMenuItems(): ReactNode[] {
  const { t } = useTranslation();
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

  return [
    <MenuItem key="mode" disableRipple onClick={(e) => e.stopPropagation()} sx={{ "&:hover": { bgcolor: "transparent" } }}>
      <ListItemIcon>{mode === "dark" ? <DarkMode fontSize="small" /> : <LightMode fontSize="small" />}</ListItemIcon>
      <ListItemText>{mode === "dark" ? t("theme.darkMode") : t("theme.lightMode")}</ListItemText>
      <Switch checked={mode === "dark"} onChange={toggleMode} edge="end" size="small" sx={{ ml: 2 }} />
    </MenuItem>,
    <MenuItem key="glass" disableRipple onClick={(e) => e.stopPropagation()} sx={{ "&:hover": { bgcolor: "transparent" } }}>
      <ListItemIcon>{frostedGlass ? <BlurOn fontSize="small" /> : <BlurOff fontSize="small" />}</ListItemIcon>
      <ListItemText>{t("theme.frostedGlass")}</ListItemText>
      <Switch checked={frostedGlass} onChange={toggleFrostedGlass} edge="end" size="small" sx={{ ml: 2 }} />
    </MenuItem>,
    <Box key="opacity" sx={{ px: 2, pt: 0.5, pb: 1.5, opacity: frostedGlass ? 1 : 0.4 }}>
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
    </Box>,
    <Box key="blur" sx={{ px: 2, pt: 0.5, pb: 1.5, opacity: frostedGlass ? 1 : 0.4 }}>
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
    </Box>,
  ];
}
