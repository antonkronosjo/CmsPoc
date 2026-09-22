import { useState, type MouseEvent } from "react";
import { Avatar, Box, ButtonBase, ListItemIcon, ListItemText, Menu, MenuItem, Slider, Switch, Typography } from "@mui/material";
import { AccountCircle, BlurOff, BlurOn, DarkMode, LightMode } from "@mui/icons-material";
import { useUser } from "../context/UserContext";
import { useThemeMode } from "../theme/ThemeModeProvider";

/// Avatar (plus display name) in the CMS header that opens a settings menu. The switches and
/// the opacity slider only call their own setter - they never close the menu - so any of
/// them can be adjusted back and forth without reopening the menu each time.
export default function UserMenu() {
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
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);

  return (
    <>
      <ButtonBase
        onClick={(e: MouseEvent<HTMLElement>) => setAnchorEl(e.currentTarget)}
        aria-label="User settings"
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
          <ListItemText>{mode === "dark" ? "Dark mode" : "Light mode"}</ListItemText>
          <Switch checked={mode === "dark"} onChange={toggleMode} edge="end" size="small" sx={{ ml: 2 }} />
        </MenuItem>
        <MenuItem disableRipple onClick={(e) => e.stopPropagation()} sx={{ "&:hover": { bgcolor: "transparent" } }}>
          <ListItemIcon>{frostedGlass ? <BlurOn fontSize="small" /> : <BlurOff fontSize="small" />}</ListItemIcon>
          <ListItemText>Frosted glass</ListItemText>
          <Switch checked={frostedGlass} onChange={toggleFrostedGlass} edge="end" size="small" sx={{ ml: 2 }} />
        </MenuItem>
        <Box sx={{ px: 2, pt: 0.5, pb: 1.5, opacity: frostedGlass ? 1 : 0.4 }}>
          <Typography variant="caption" color="text.secondary">
            Opacity: {Math.round(glassOpacity * 100)}%
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
            Blur: {glassBlur}px
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
