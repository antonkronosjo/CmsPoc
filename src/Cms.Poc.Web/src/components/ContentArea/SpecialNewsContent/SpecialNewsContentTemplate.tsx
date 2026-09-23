import { Card, CardContent, Chip, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { getProperty, type ContentTemplateProps } from "../types";

const ACCENT_COLOR = "#ED6C02";

export default function SpecialNewsContentTemplate({ content }: ContentTemplateProps) {
  const { t } = useTranslation();
  const heading = getProperty(content, "Heading") || content.name;
  const intro = getProperty(content, "Intro");

  return (
    <Card sx={{ height: "100%", borderColor: ACCENT_COLOR, borderWidth: 2, borderStyle: "solid" }}>
      <CardContent>
        <Chip size="small" label={t("contentType.specialNews")} sx={{ mb: 1 }} />
        <Typography variant="h6" gutterBottom>
          {heading}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {intro}
        </Typography>
      </CardContent>
    </Card>
  );
}
