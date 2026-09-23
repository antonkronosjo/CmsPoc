import type { ReactNode } from "react";
import { Link as RouterLink, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Alert, Button, Container, Stack, Typography } from "@mui/material";
import { ArrowBack, Edit } from "@mui/icons-material";
import { useTranslation } from "react-i18next";
import { api, type ContentSummaryDto } from "../../api/client";
import { useLanguage } from "../../hooks/useLanguage";
import { useUser } from "../../context/UserContext";

interface ContentPageShellProps {
  /// The content type this page renders; any other type (or unpublished content) is shown as not found.
  contentTypeKey: string;
  children: (content: ContentSummaryDto) => ReactNode;
}

/// Shared plumbing for the public content pages: reads :id from the route,
/// loads the published content, and handles loading / not-found states so each
/// page only has to render its own content type.
export default function ContentPageShell({ contentTypeKey, children }: ContentPageShellProps) {
  const { t } = useTranslation();
  const { id } = useParams();
  const { language } = useLanguage();
  const { isAuthenticated } = useUser();
  const contentId = Number(id);

  const { data, isLoading } = useQuery({
    queryKey: ["content-summary", contentId, language],
    queryFn: () => api.getContentSummary(contentId, language),
    enabled: Number.isInteger(contentId),
    retry: false,
  });

  // Found only when the live version itself has this translation. A translation that exists
  // only in a newer, unpublished version is not available to the public.
  const found =
    data &&
    data.contentTypeKey === contentTypeKey &&
    data.livePublishedVersionNumber != null &&
    data.versionNumber === data.livePublishedVersionNumber;

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Stack direction="row" sx={{ justifyContent: "space-between", mb: 2 }}>
        <Button component={RouterLink} to={`/${language}`} startIcon={<ArrowBack />}>
          {t("contentPageShell.back")}
        </Button>
        {found && isAuthenticated && (
          <Button component={RouterLink} to={`/cms/edit/${contentId}?lang=${language}`} startIcon={<Edit />} variant="outlined">
            {t("contentPageShell.edit")}
          </Button>
        )}
      </Stack>
      {isLoading ? (
        <Typography color="text.secondary">{t("contentPageShell.loading")}</Typography>
      ) : found ? (
        children(data)
      ) : (
        <Alert severity="warning">{t("contentPageShell.notFound")}</Alert>
      )}
    </Container>
  );
}
