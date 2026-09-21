import { Card, Stack, Typography } from "@mui/material";
import { useNavigate } from "react-router-dom";
import ContentBrowseList from "../../components/ContentBrowseList";

export default function CmsBrowsePage() {
  const navigate = useNavigate();

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Browse content</Typography>
      <Card sx={{ p: 3 }}>
        <ContentBrowseList showTypeFilter onSelect={(item) => navigate(`/cms/edit/${item.id}`)} />
      </Card>
    </Stack>
  );
}
