import ContentTypeLandingPage from "./ContentTypeLandingPage";

/// All published news, including special news.
export default function NewsListPage() {
  return <ContentTypeLandingPage titleKey="newsListPage.heading" contentTypeKey={["NewsContent", "SpecialNewsContent"]} />;
}
