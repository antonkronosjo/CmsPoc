import { useTranslation } from "react-i18next";
import ContentTypeLandingPage from "./ContentTypeLandingPage";

/// All published events - filterable by start date.
export default function EventsListPage() {
  const { t } = useTranslation();

  return (
    <ContentTypeLandingPage
      titleKey="eventsListPage.heading"
      contentTypeKey="EventContent"
      filterFields={[{ kind: "dateRange", label: t("contentListFilters.startDate"), property: "StartDate" }]}
    />
  );
}
