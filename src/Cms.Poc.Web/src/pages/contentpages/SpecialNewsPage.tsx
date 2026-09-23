import { Chip, Stack, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { getProperty } from "../../components/ContentArea/types";
import MarkdownContent from "../../components/MarkdownContent";
import ContentPageShell from "./ContentPageShell";

export default function SpecialNewsPage() {
  const { t } = useTranslation();
  return (
    <ContentPageShell contentTypeKey="SpecialNewsContent">
      {(content) => (
        <Stack spacing={2}>
          <Chip size="small" label={t("contentType.specialNews")} sx={{ alignSelf: "flex-start" }} />
          <Typography variant="h3" component="h1">
            {getProperty(content, "Heading") || content.name}
          </Typography>
          <Typography variant="h6" component="p" sx={{ fontWeight: 500, whiteSpace: "pre-line" }}>
            {getProperty(content, "Intro")}
          </Typography>
          <MarkdownContent>{getProperty(content, "Body")}</MarkdownContent>
          <MarkdownContent>{getProperty(content, "SpecialBody")}</MarkdownContent>
        </Stack>
      )}
    </ContentPageShell>
  );
}
