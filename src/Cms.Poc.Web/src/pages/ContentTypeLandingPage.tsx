import { Container } from "@mui/material";
import { useTranslation } from "react-i18next";
import ContentList from "../components/ContentList/ContentList";

interface ContentTypeLandingPageProps {
  /// i18n key for the page heading.
  titleKey: string;
  /// One or more content type keys to show every published item of (no "latest N" limit).
  contentTypeKey: string | string[];
}

/// Shared shell for a "browse all of this content type" public page, e.g. /news or /events.
export default function ContentTypeLandingPage({ titleKey, contentTypeKey }: ContentTypeLandingPageProps) {
  const { t } = useTranslation();

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <ContentList
        title={t(titleKey)}
        contentTypeKey={contentTypeKey}
        noContentText={t("common.noContentFound")}
        titleVariant="h4"
        titleComponent="h1"
      />
    </Container>
  );
}
