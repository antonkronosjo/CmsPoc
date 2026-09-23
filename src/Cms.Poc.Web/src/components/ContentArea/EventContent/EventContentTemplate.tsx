import { Card, CardContent, Chip, Typography } from "@mui/material";
import dayjs from "dayjs";
import { useTranslation } from "react-i18next";
import { getProperty, type ContentTemplateProps } from "../types";

const ACCENT_COLOR = "#5B6EF5";

export default function EventContentTemplate({ content }: ContentTemplateProps) {
  const { t } = useTranslation();
  const title = getProperty(content, "Title") || content.name;
  const description = getProperty(content, "Description");
  const startDate = getProperty(content, "StartDate");

  return (
    <Card sx={{ height: "100%", borderTop: `4px solid ${ACCENT_COLOR}` }}>
      <CardContent>
        <Chip size="small" label={t("contentType.event")} sx={{ mb: 1 }} />
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
