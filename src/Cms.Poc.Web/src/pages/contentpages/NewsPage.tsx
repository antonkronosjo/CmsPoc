import { Chip, Stack, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { getProperty } from "../../components/ContentArea/types";
import ContentPageShell from "./ContentPageShell";

export default function NewsPage() {
  const { t } = useTranslation();
  return (
    <ContentPageShell contentTypeKey="NewsContent">
      {(content) => (
        <Stack spacing={2}>
          <Chip size="small" label={t("contentType.news")} sx={{ alignSelf: "flex-start" }} />
          <Typography variant="h3" component="h1">
            {getProperty(content, "Heading") || content.name}
          </Typography>
          <Typography variant="h6" component="p" sx={{ fontWeight: 500, whiteSpace: "pre-line" }}>
            {getProperty(content, "Intro")}
          </Typography>
          <Typography sx={{ whiteSpace: "pre-line" }}>{getProperty(content, "Body")}</Typography>
        </Stack>
      )}
    </ContentPageShell>
  );
}
