import { Card, CardContent, Chip, Stack, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { EventStatusChip, formatEventRange } from "../../EventDateInfo";
import { getProperty, type ContentTemplateProps } from "../types";

const ACCENT_COLOR = "#5B6EF5";

export default function EventContentTemplate({ content }: ContentTemplateProps) {
  const { t } = useTranslation();
  const title = getProperty(content, "Title") || content.name;
  const intro = getProperty(content, "Intro");
  const startDate = getProperty(content, "StartDate");
  const endDate = getProperty(content, "EndDate");

  return (
    <Card sx={{ height: "100%", borderTop: `4px solid ${ACCENT_COLOR}` }}>
      <CardContent>
        <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center", mb: 1 }}>
          <Chip size="small" label={t("contentType.event")} />
          <EventStatusChip startDate={startDate} endDate={endDate || undefined} />
        </Stack>
        <Typography variant="h6">{title}</Typography>
        {startDate && (
          <Typography variant="subtitle2" color="text.secondary" gutterBottom>
            {formatEventRange(startDate, endDate || undefined)}
          </Typography>
        )}
        <Typography variant="body2" color="text.secondary">
          {intro}
        </Typography>
      </CardContent>
    </Card>
  );
}
