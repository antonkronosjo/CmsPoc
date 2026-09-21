import { Card, CardContent, Chip, Typography } from "@mui/material";
import dayjs from "dayjs";
import { getProperty, type ContentTemplateProps } from "../types";

export default function EventContentTemplate({ content }: ContentTemplateProps) {
  const title = getProperty(content, "Title") || content.name;
  const description = getProperty(content, "Description");
  const startDate = getProperty(content, "StartDate");

  return (
    <Card sx={{ height: "100%" }}>
      <CardContent>
        <Chip size="small" color="primary" label="Event" sx={{ mb: 1 }} />
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
