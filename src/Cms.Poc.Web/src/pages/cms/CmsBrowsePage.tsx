import { Stack, Typography } from "@mui/material";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import ContentBrowseList from "../../components/ContentBrowseList";

export default function CmsBrowsePage() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  return (
    <Stack spacing={2}>
      <Typography variant="h5">{t("cmsBrowsePage.heading")}</Typography>
      <ContentBrowseList showTypeFilter onSelect={(item) => navigate(`/cms/edit/${item.id}`)} />
    </Stack>
  );
}
