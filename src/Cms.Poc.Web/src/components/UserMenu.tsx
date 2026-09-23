import { useState, type MouseEvent } from "react";
import { Avatar, ButtonBase, ListItemIcon, ListItemText, Menu, MenuItem, Switch, Typography } from "@mui/material";
import { AccountCircle, Translate } from "@mui/icons-material";
import { useTranslation } from "react-i18next";
import { useUser } from "../context/UserContext";
import { useUiLanguage } from "../hooks/useUiLanguage";
import { useAppearanceMenuItems } from "./useAppearanceMenuItems";

/// Avatar (plus display name) in the CMS header that opens a settings menu: the shared
/// appearance controls plus the CMS's own UI language switch. The switches only call their
/// own setter - they never close the menu - so any of them can be adjusted back and forth
/// without reopening the menu each time.
export default function UserMenu() {
  const { t } = useTranslation();
  const { displayName } = useUser();
  const { uiLanguage, setUiLanguage } = useUiLanguage();
  const appearanceItems = useAppearanceMenuItems();
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
        {appearanceItems}
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
      </Menu>
    </>
  );
}
