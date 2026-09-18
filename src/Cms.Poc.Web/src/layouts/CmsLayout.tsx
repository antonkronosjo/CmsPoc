import { Outlet, Link as RouterLink, useLocation } from "react-router-dom";
import { Box, Drawer, List, ListItemButton, ListItemIcon, ListItemText, Toolbar } from "@mui/material";
import { Add, ViewList } from "@mui/icons-material";

const NAV_ICON_MIN_WIDTH = 32;

const DRAWER_WIDTH = 220;

const NAV_ITEMS = [
  { to: "/cms", label: "Browse", icon: <ViewList />, exact: true },
  { to: "/cms/create", label: "Create", icon: <Add />, exact: false },
];

export default function CmsLayout() {
  const location = useLocation();

  return (
    <Box sx={{ display: "flex", flex: 1 }}>
      <Drawer
        variant="permanent"
        sx={{
          width: DRAWER_WIDTH,
          flexShrink: 0,
          [`& .MuiDrawer-paper`]: {
            width: DRAWER_WIDTH,
            boxSizing: "border-box",
            position: "static",
            border: "none",
            backgroundColor: "background.paper",
          },
        }}
      >
        <Toolbar />
        <List sx={{ px: 1.5, py: 1, display: "flex", flexDirection: "column", gap: 0.25 }}>
          {NAV_ITEMS.map((item) => {
            const selected = item.exact ? location.pathname === item.to : location.pathname.startsWith(item.to);
            return (
              <ListItemButton
                key={item.to}
                component={RouterLink}
                to={item.to}
                selected={selected}
                sx={{
                  borderRadius: 2,
                  py: 0.75,
                  "&.Mui-selected": {
                    bgcolor: (t) => (t.palette.mode === "light" ? "rgba(91,110,245,0.08)" : "rgba(138,147,255,0.14)"),
                    "&:hover": {
                      bgcolor: (t) => (t.palette.mode === "light" ? "rgba(91,110,245,0.12)" : "rgba(138,147,255,0.2)"),
                    },
                  },
                }}
              >
                <ListItemIcon sx={{ minWidth: NAV_ICON_MIN_WIDTH, color: selected ? "primary.main" : "inherit" }}>
                  {item.icon}
                </ListItemIcon>
                <ListItemText
                  primary={item.label}
                  slotProps={{
                    primary: { sx: { fontWeight: selected ? 600 : 500, color: selected ? "primary.main" : "text.primary" } },
                  }}
                />
                {selected && (
                  <Box
                    sx={{ width: 6, height: 6, borderRadius: "50%", bgcolor: "primary.main", flexShrink: 0 }}
                  />
                )}
              </ListItemButton>
            );
          })}
        </List>
      </Drawer>
      <Box component="main" sx={{ flex: 1, p: 3, minWidth: 0 }}>
        <Outlet />
      </Box>
    </Box>
  );
}
