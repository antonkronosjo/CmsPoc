import { Card, Container, Stack, Typography } from "@mui/material";
import ContentBrowseList from "../components/ContentBrowseList";
import { useLanguage } from "../context/LanguageContext";

export default function HomePage() {
  const { language } = useLanguage();

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
      <Card sx={{ p: 3 }}>
        <ContentBrowseList language={language} publishedOnly />
      </Card>
    </Container>
  );
}
