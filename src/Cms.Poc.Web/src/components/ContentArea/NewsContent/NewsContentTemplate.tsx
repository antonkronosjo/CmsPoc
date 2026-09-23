import { Card, CardContent, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import ContentTypeChip from "../../ContentTypeChip";
import { useContentTypeColor } from "../../../hooks/useContentTypes";
import { getProperty, type ContentTemplateProps } from "../types";

export default function NewsContentTemplate({ content }: ContentTemplateProps) {
  const { t } = useTranslation();
  const heading = getProperty(content, "Heading") || content.name;
  const body = getProperty(content, "Body");
  const color = useContentTypeColor(content.contentTypeKey);

  return (
    <Card sx={{ height: "100%", borderTop: color ? `4px solid ${color}` : undefined }}>
      <CardContent>
        <ContentTypeChip contentTypeKey={content.contentTypeKey} label={t("contentType.news")} sx={{ mb: 1 }} />
        <Typography variant="h6" gutterBottom>
          {heading}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {body}
        </Typography>
      </CardContent>
    </Card>
  );
}
