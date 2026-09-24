import { useState, type MouseEvent } from "react";
import { IconButton, Menu, Tooltip } from "@mui/material";
import { Contrast } from "@mui/icons-material";
import { useTranslation } from "react-i18next";
import { useAppearanceMenuItems } from "./useAppearanceMenuItems";

/// Icon-only button in the public site's header that opens the theme controls.
export default function AppearanceMenu() {
  const { t } = useTranslation();
  const items = useAppearanceMenuItems();
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);

  return (
    <>
      <Tooltip title={t("nav.appearance")}>
        <IconButton
          onClick={(e: MouseEvent<HTMLElement>) => setAnchorEl(e.currentTarget)}
          aria-label={t("nav.appearance")}
          color="inherit"
        >
          <Contrast />
        </IconButton>
      </Tooltip>
      <Menu
        anchorEl={anchorEl}
        open={!!anchorEl}
        onClose={() => setAnchorEl(null)}
        anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
        transformOrigin={{ vertical: "top", horizontal: "right" }}
      >
        {items}
      </Menu>
    </>
  );
}
