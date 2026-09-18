import { Route, Routes } from "react-router-dom";
import AppLayout from "./layouts/AppLayout";
import CmsLayout from "./layouts/CmsLayout";
import HomePage from "./pages/HomePage";
import CmsBrowsePage from "./pages/cms/CmsBrowsePage";
import CmsCreatePage from "./pages/cms/CmsCreatePage";
import CmsEditPage from "./pages/cms/CmsEditPage";

/// Single source of truth for the route tree. Adding a page elsewhere in
/// the app means adding one file under `pages/` and one <Route> line here.
export default function AppRoutes() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/cms" element={<CmsLayout />}>
          <Route index element={<CmsBrowsePage />} />
          <Route path="create" element={<CmsCreatePage />} />
          <Route path="edit/:contentId" element={<CmsEditPage />} />
          <Route path="edit/:contentId/:versionId" element={<CmsEditPage />} />
        </Route>
      </Route>
    </Routes>
  );
}
