import { useTranslation } from "react-i18next";
import ContentTypeLandingPage from "./ContentTypeLandingPage";

/// All published news, including special news - filterable by content type and publish date.
export default function NewsListPage() {
  const { t } = useTranslation();

  return (
    <ContentTypeLandingPage
      titleKey="newsListPage.heading"
      contentTypeKey={["NewsContent", "SpecialNewsContent"]}
      filterFields={[
        {
          kind: "contentType",
          label: t("contentListFilters.contentType"),
          options: [
            { value: "NewsContent", label: t("contentType.news") },
            { value: "SpecialNewsContent", label: t("contentType.specialNews") },
          ],
        },
        { kind: "dateRange", label: t("contentListFilters.publishDate") },
      ]}
    />
  );
}
