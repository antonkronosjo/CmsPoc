import { Component, type ReactNode } from "react";
import { Box, Button, Stack, Typography } from "@mui/material";
import { withTranslation, type WithTranslation } from "react-i18next";

interface ErrorBoundaryProps extends WithTranslation {
  children: ReactNode;
}

interface ErrorBoundaryState {
  error: Error | undefined;
}

/// Catches render-time exceptions anywhere below it so a bug in one page shows a
/// recoverable message instead of a blank white screen. React only supports this via
/// a class component (getDerivedStateFromError has no hook equivalent), so translation
/// comes in via the withTranslation HOC rather than the useTranslation hook.
class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  state: ErrorBoundaryState = { error: undefined };

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { error };
  }

  render() {
    const { t } = this.props;
    if (this.state.error) {
      return (
        <Box sx={{ display: "flex", alignItems: "center", justifyContent: "center", minHeight: "100vh", p: 3 }}>
          <Stack spacing={2} sx={{ maxWidth: 420, textAlign: "center", alignItems: "center" }}>
            <Typography variant="h5">{t("errorBoundary.title")}</Typography>
            <Typography variant="body2" color="text.secondary">
              {this.state.error.message || t("errorBoundary.body")}
            </Typography>
            <Button variant="contained" onClick={() => window.location.reload()}>
              {t("common.reload")}
            </Button>
          </Stack>
        </Box>
      );
    }
    return this.props.children;
  }
}

const TranslatedErrorBoundary = withTranslation()(ErrorBoundary);
export default TranslatedErrorBoundary;
