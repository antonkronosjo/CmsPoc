import { useState, type MouseEvent } from "react";
import { Avatar, ButtonBase, ListItemIcon, ListItemText, Menu, MenuItem, Typography } from "@mui/material";
import { AccountCircle, DarkMode, LightMode } from "@mui/icons-material";
import { useUser } from "../context/UserContext";
import { useThemeMode } from "../theme/ThemeModeProvider";

/// Avatar (plus display name) in the CMS header that opens a settings menu. Dark/light mode
/// lives here rather than as its own header icon so the header only grows one more setting,
/// not one more icon, as settings are added later.
export default function UserMenu() {
  const { displayName } = useUser();
  const { mode, toggleMode } = useThemeMode();
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
        <MenuItem
          onClick={() => {
            toggleMode();
            setAnchorEl(null);
          }}
        >
          <ListItemIcon>{mode === "dark" ? <LightMode fontSize="small" /> : <DarkMode fontSize="small" />}</ListItemIcon>
          <ListItemText>{mode === "dark" ? "Light mode" : "Dark mode"}</ListItemText>
        </MenuItem>
      </Menu>
    </>
  );
}
