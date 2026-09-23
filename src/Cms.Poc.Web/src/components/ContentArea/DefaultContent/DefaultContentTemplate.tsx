import { Card, CardContent, Chip, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import type { ContentTemplateProps } from "../types";

/// Fallback card for content types without their own template.
export default function DefaultContentTemplate({ content }: ContentTemplateProps) {
  const { t } = useTranslation();

  return (
    <Card sx={{ height: "100%" }}>
      <CardContent>
        <Chip size="small" label={content.contentTypeKey} sx={{ mb: 1 }} />
        <Typography variant="h6">{content.name || <em>{t("common.untitled")}</em>}</Typography>
      </CardContent>
    </Card>
  );
}
