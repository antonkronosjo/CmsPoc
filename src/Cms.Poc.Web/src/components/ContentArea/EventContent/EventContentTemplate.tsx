import { Card, CardContent, Typography } from "@mui/material";
import dayjs from "dayjs";
import ContentTypeChip from "../../ContentTypeChip";
import { useContentTypeColor } from "../../../hooks/useContentTypes";
import { getProperty, type ContentTemplateProps } from "../types";

export default function EventContentTemplate({ content }: ContentTemplateProps) {
  const title = getProperty(content, "Title") || content.name;
  const description = getProperty(content, "Description");
  const startDate = getProperty(content, "StartDate");
  const color = useContentTypeColor(content.contentTypeKey);

  return (
    <Card sx={{ height: "100%", borderTop: color ? `4px solid ${color}` : undefined }}>
      <CardContent>
        <ContentTypeChip contentTypeKey={content.contentTypeKey} label="Event" sx={{ mb: 1 }} />
        <Typography variant="h6">{title}</Typography>
        {startDate && (
          <Typography variant="subtitle2" color="text.secondary" gutterBottom>
            {dayjs(startDate).format("dddd D MMMM YYYY, HH:mm")}
          </Typography>
        )}
        <Typography variant="body2" color="text.secondary">
          {description}
        </Typography>
      </CardContent>
    </Card>
  );
}
