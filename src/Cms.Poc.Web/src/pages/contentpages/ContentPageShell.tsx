import type { ReactNode } from "react";
import { Link as RouterLink, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Alert, Button, Container, Stack, Typography } from "@mui/material";
import { ArrowBack, Edit } from "@mui/icons-material";
import { api, type ContentSummaryDto } from "../../api/client";
import { useLanguage } from "../../context/LanguageContext";

interface ContentPageShellProps {
  /// The content type this page renders; any other type (or unpublished content) is shown as not found.
  contentTypeKey: string;
  children: (content: ContentSummaryDto) => ReactNode;
}

/// Shared plumbing for the public content pages: reads :id from the route,
/// loads the published content, and handles loading / not-found states so each
/// page only has to render its own content type.
export default function ContentPageShell({ contentTypeKey, children }: ContentPageShellProps) {
  const { id } = useParams();
  const { language } = useLanguage();
  const contentId = Number(id);

  const { data, isLoading, isError } = useQuery({
    queryKey: ["content-summary", contentId, language],
    queryFn: () => api.getContentSummary(contentId, language),
    enabled: Number.isInteger(contentId),
    retry: false,
  });

  const found = data && data.contentTypeKey === contentTypeKey && data.livePublishedVersionNumber != null;

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Stack direction="row" sx={{ justifyContent: "space-between", mb: 2 }}>
        <Button component={RouterLink} to="/" startIcon={<ArrowBack />}>
          Back
        </Button>
        {found && (
          <Button component={RouterLink} to={`/cms/edit/${contentId}`} startIcon={<Edit />} variant="outlined">
            Edit
          </Button>
        )}
      </Stack>
      {isLoading ? (
        <Typography color="text.secondary">Loading…</Typography>
      ) : found ? (
        children(data)
      ) : (
        <Alert severity="warning">{isError || !data ? "Content not found." : "This content is not available."}</Alert>
      )}
    </Container>
  );
}
