import { Navigate, Route, createBrowserRouter, createRoutesFromElements } from "react-router-dom";
import AppLayout from "./layouts/AppLayout";
import CmsLayout from "./layouts/CmsLayout";
import HomePage from "./pages/HomePage";
import PublicLanguageRoute from "./pages/PublicLanguageRoute";
import { useLanguages } from "./hooks/useLanguages";
import CmsBrowsePage from "./pages/cms/CmsBrowsePage";
import CmsCreatePage from "./pages/cms/CmsCreatePage";
import CmsEditPage from "./pages/cms/CmsEditPage";
import CmsSettingsPage from "./pages/cms/CmsSettingsPage";
import EventPage from "./pages/contentpages/EventPage";
import NewsPage from "./pages/contentpages/NewsPage";
import SpecialNewsPage from "./pages/contentpages/SpecialNewsPage";

/// Single source of truth for the route tree. Adding a page elsewhere in
/// the app means adding one file under `pages/` and one <Route> line here.
/// The public site lives under /:language, so the bare root goes to the default language.
function RedirectToDefaultLanguage() {
  const { loaded, defaultLanguage } = useLanguages();
  return loaded ? <Navigate to={`/${defaultLanguage}`} replace /> : null;
}

// A data router (rather than plain <BrowserRouter>) is required for useBlocker,
// which CmsEditPage uses to confirm navigation away from unsaved changes.
export const router = createBrowserRouter(
  createRoutesFromElements(
    <Route element={<AppLayout />}>
      <Route path="/" element={<RedirectToDefaultLanguage />} />
      <Route path="/:language" element={<PublicLanguageRoute />}>
        <Route index element={<HomePage />} />
        <Route path="event/:id" element={<EventPage />} />
        <Route path="news/:id" element={<NewsPage />} />
        <Route path="specialnews/:id" element={<SpecialNewsPage />} />
      </Route>
      <Route path="/cms" element={<CmsLayout />}>
        <Route index element={<CmsBrowsePage />} />
        <Route path="create" element={<CmsCreatePage />} />
        <Route path="edit/:contentId" element={<CmsEditPage />} />
        <Route path="edit/:contentId/:versionId" element={<CmsEditPage />} />
        <Route path="settings" element={<CmsSettingsPage />} />
      </Route>
    </Route>,
  ),
);
