import { Card, Container, Stack, Typography } from "@mui/material";
import { useNavigate } from "react-router-dom";
import ContentBrowseList from "../components/ContentBrowseList";
import { useLanguage } from "../context/LanguageContext";

export default function HomePage() {
  const { language } = useLanguage();
  const navigate = useNavigate();

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={1} sx={{ mb: 3 }}>
        <Typography variant="h4" component="h1">
          All content
        </Typography>
        <Typography color="text.secondary">
          Every piece of content created so far. Head to the CMS section to browse with filters, create new content, or
          edit an existing item.
        </Typography>
      </Stack>
      <Card sx={{ p: 3 }}>
        <ContentBrowseList language={language} onSelect={(item) => navigate(`/cms/edit/${item.id}`)} />
      </Card>
    </Container>
  );
}
