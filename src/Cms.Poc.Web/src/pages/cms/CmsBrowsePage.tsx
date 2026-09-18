import { Paper, Stack, Typography } from "@mui/material";
import { useNavigate } from "react-router-dom";
import ContentBrowseList from "../../components/ContentBrowseList";
import { useLanguage } from "../../context/LanguageContext";

export default function CmsBrowsePage() {
  const { language } = useLanguage();
  const navigate = useNavigate();

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Browse content</Typography>
      <Paper sx={{ p: 2 }}>
        <ContentBrowseList language={language} showTypeFilter onSelect={(item) => navigate(`/cms/edit/${item.id}`)} />
      </Paper>
    </Stack>
  );
}
