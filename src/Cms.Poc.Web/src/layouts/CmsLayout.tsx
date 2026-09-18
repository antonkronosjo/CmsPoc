import { Outlet, Link as RouterLink, useLocation } from "react-router-dom";
import { Box, Drawer, List, ListItemButton, ListItemIcon, ListItemText, Toolbar } from "@mui/material";
import { Add, ViewList } from "@mui/icons-material";

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
          [`& .MuiDrawer-paper`]: { width: DRAWER_WIDTH, boxSizing: "border-box", position: "static" },
        }}
      >
        <Toolbar />
        <List>
          {NAV_ITEMS.map((item) => {
            const selected = item.exact ? location.pathname === item.to : location.pathname.startsWith(item.to);
            return (
              <ListItemButton key={item.to} component={RouterLink} to={item.to} selected={selected}>
                <ListItemIcon>{item.icon}</ListItemIcon>
                <ListItemText primary={item.label} />
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
