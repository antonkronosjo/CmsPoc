import { useQuery } from "@tanstack/react-query";
import { Container, Stack, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { api } from "../api/client";
import ContentArea from "../components/ContentArea/ContentArea";
import { useLanguage } from "../hooks/useLanguage";

const PAGE_SIZE = 200;

export default function HomePage() {
  const { t } = useTranslation();
  const { language } = useLanguage();

  const { data, isFetching } = useQuery({
    queryKey: ["content-search", "", language, "", 1, true, PAGE_SIZE],
    queryFn: () => api.searchContent("", language, { pageSize: PAGE_SIZE, publishedOnly: true }),
  });
  const items = data?.items ?? [];

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={1} sx={{ mb: 3 }}>
        <Typography variant="h4" component="h1">
          {t("homePage.heading")}
        </Typography>
        <Typography color="text.secondary">{t("homePage.description")}</Typography>
      </Stack>
      <ContentArea content={items} />
      {!isFetching && items.length === 0 && (
        <Typography variant="body2" color="text.secondary">
          {t("common.noContentFound")}
        </Typography>
      )}
    </Container>
  );
}
