import { Box, CircularProgress } from "@mui/material";
import type { ReactNode } from "react";

interface LoadingOverlayProps {
  loading: boolean;
  children: ReactNode;
}

/// Generic "this content is refreshing" wrapper: instead of swapping the content out for a
/// skeleton, it stays on screen blurred (with a smooth transition) behind a centered spinner.
/// The spinner is positioned with the sticky-inside-a-centered-grid-cell trick (`position:
/// sticky` with both `top` and `bottom` set) so on a wrapped area taller than the viewport it
/// tracks to stay visible, always padded away from the viewport edges, rather than scrolling
/// out of view like a plain centered absolute element would.
export default function LoadingOverlay({ loading, children }: LoadingOverlayProps) {
  return (
    <Box sx={{ position: "relative" }}>
      <Box
        sx={{
          filter: loading ? "blur(3px)" : "none",
          transition: "filter 200ms ease",
          pointerEvents: loading ? "none" : undefined,
        }}
      >
        {children}
      </Box>
      {loading && (
        <Box sx={{ position: "absolute", inset: 0, display: "grid", placeItems: "center" }}>
          <CircularProgress sx={{ position: "sticky", top: 2, bottom: 2 }} />
        </Box>
      )}
    </Box>
  );
}
