import { Card, CardContent, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import ContentTypeChip from "../../ContentTypeChip";
import { useContentTypeColor } from "../../../hooks/useContentTypes";
import type { ContentTemplateProps } from "../types";

/// Fallback card for content types without their own template.
export default function DefaultContentTemplate({ content }: ContentTemplateProps) {
  const { t } = useTranslation();
  const color = useContentTypeColor(content.contentTypeKey);

  return (
    <Card sx={{ height: "100%", borderTop: color ? `4px solid ${color}` : undefined }}>
      <CardContent>
        <ContentTypeChip contentTypeKey={content.contentTypeKey} sx={{ mb: 1 }} />
        <Typography variant="h6">{content.name || <em>{t("common.untitled")}</em>}</Typography>
      </CardContent>
    </Card>
  );
}
