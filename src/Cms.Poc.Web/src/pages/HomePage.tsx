import { useQuery } from "@tanstack/react-query";
import { Container, Stack, Typography } from "@mui/material";
import { api } from "../api/client";
import ContentArea from "../components/ContentArea/ContentArea";
import { useLanguage } from "../context/LanguageContext";

const PAGE_SIZE = 200;

export default function HomePage() {
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
          Published content
        </Typography>
        <Typography color="text.secondary">
          Everything currently live. Head to the CMS section to manage drafts, publish new versions, or edit existing
          content.
        </Typography>
      </Stack>
      <ContentArea content={items} />
      {!isFetching && items.length === 0 && (
        <Typography variant="body2" color="text.secondary">
          No content found.
        </Typography>
      )}
    </Container>
  );
}
