import { Container, Divider, Stack, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import ContentList from "../components/ContentList/ContentList";

const LATEST_COUNT = 3;

export default function HomePage() {
  const { t } = useTranslation();

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={1} sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1">
          {t("homePage.heading")}
        </Typography>
        <Typography color="text.secondary">{t("homePage.description")}</Typography>
      </Stack>
      <Stack spacing={4} divider={<Divider />}>
        <ContentList title={t("homePage.latestItems")} count={LATEST_COUNT} noContentText={t("common.noContentFound")} />
        <ContentList
          title={t("homePage.latestNews")}
          contentTypeKey="NewsContent"
          count={LATEST_COUNT}
          noContentText={t("common.noContentFound")}
          link={{ to: "news", label: t("homePage.allNews") }}
        />
        <ContentList
          title={t("homePage.latestEvents")}
          contentTypeKey="EventContent"
          count={LATEST_COUNT}
          noContentText={t("common.noContentFound")}
          link={{ to: "events", label: t("homePage.allEvents") }}
        />
      </Stack>
    </Container>
  );
}
