import { Component, type ReactNode } from "react";
import { Box, Button, Stack, Typography } from "@mui/material";

interface ErrorBoundaryProps {
  children: ReactNode;
}

interface ErrorBoundaryState {
  error: Error | undefined;
}

/// Catches render-time exceptions anywhere below it so a bug in one page shows a
/// recoverable message instead of a blank white screen. React only supports this via
/// a class component (getDerivedStateFromError has no hook equivalent).
export default class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  state: ErrorBoundaryState = { error: undefined };

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { error };
  }

  render() {
    if (this.state.error) {
      return (
        <Box sx={{ display: "flex", alignItems: "center", justifyContent: "center", minHeight: "100vh", p: 3 }}>
          <Stack spacing={2} sx={{ maxWidth: 420, textAlign: "center", alignItems: "center" }}>
            <Typography variant="h5">Something went wrong</Typography>
            <Typography variant="body2" color="text.secondary">
              {this.state.error.message || "An unexpected error occurred."}
            </Typography>
            <Button variant="contained" onClick={() => window.location.reload()}>
              Reload
            </Button>
          </Stack>
        </Box>
      );
    }
    return this.props.children;
  }
}
