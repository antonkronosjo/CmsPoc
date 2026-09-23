import { useTranslation } from "react-i18next";
import { UI_LANGUAGES, storeUiLanguage, type UiLanguage } from "../i18n/i18n";

/// The CMS chrome's own language (menus, buttons, labels), independent of the content
/// languages being edited. Persisted so a CMS editor can work in an English UI while
/// editing Swedish content, or vice versa. The public site doesn't use this: its UI
/// language always follows the content language in the URL.
export function useUiLanguage() {
  const { i18n } = useTranslation();
  const uiLanguage = (i18n.resolvedLanguage ?? i18n.language) as UiLanguage;

  const setUiLanguage = (next: UiLanguage) => {
    storeUiLanguage(next);
    void i18n.changeLanguage(next);
  };

  return { uiLanguage, setUiLanguage, uiLanguages: UI_LANGUAGES };
}
