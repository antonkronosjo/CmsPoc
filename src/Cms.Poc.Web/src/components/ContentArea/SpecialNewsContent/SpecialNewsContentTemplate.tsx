import { Card, CardContent, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import ContentTypeChip from "../../ContentTypeChip";
import { useContentTypeColor } from "../../../hooks/useContentTypes";
import { getProperty, type ContentTemplateProps } from "../types";

export default function SpecialNewsContentTemplate({ content }: ContentTemplateProps) {
  const { t } = useTranslation();
  const heading = getProperty(content, "Heading") || content.name;
  const body = getProperty(content, "Body");
  const specialBody = getProperty(content, "SpecialBody");
  const color = useContentTypeColor(content.contentTypeKey);

  return (
    <Card sx={{ height: "100%", borderColor: color ?? "warning.main", borderWidth: 2, borderStyle: "solid" }}>
      <CardContent>
        <ContentTypeChip contentTypeKey={content.contentTypeKey} label={t("contentType.specialNews")} sx={{ mb: 1 }} />
        <Typography variant="h6" gutterBottom>
          {heading}
        </Typography>
        <Typography variant="body2" color="text.secondary" gutterBottom>
          {body}
        </Typography>
        <Typography variant="body2">{specialBody}</Typography>
      </CardContent>
    </Card>
  );
}
