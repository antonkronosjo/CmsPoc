import { Stack, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { getProperty } from "../../components/ContentArea/types";
import ContentTypeChip from "../../components/ContentTypeChip";
import ContentPageShell from "./ContentPageShell";

export default function SpecialNewsPage() {
  const { t } = useTranslation();
  return (
    <ContentPageShell contentTypeKey="SpecialNewsContent">
      {(content) => (
        <Stack spacing={2}>
          <ContentTypeChip contentTypeKey={content.contentTypeKey} label={t("contentType.specialNews")} sx={{ alignSelf: "flex-start" }} />
          <Typography variant="h3" component="h1">
            {getProperty(content, "Heading") || content.name}
          </Typography>
          <Typography sx={{ whiteSpace: "pre-line" }}>{getProperty(content, "Body")}</Typography>
          <Typography sx={{ whiteSpace: "pre-line", fontWeight: 500 }}>{getProperty(content, "SpecialBody")}</Typography>
        </Stack>
      )}
    </ContentPageShell>
  );
}
