import { useTranslation } from "react-i18next";
import { UI_LANGUAGES, type UiLanguage } from "../i18n/i18n";

/// The UI chrome's own language (menus, buttons, labels), independent of the public
/// site's content language (see useLanguage). Persisted separately so a CMS editor can
/// work in an English UI while editing Swedish content, or vice versa.
export function useUiLanguage() {
  const { i18n } = useTranslation();
  const uiLanguage = (i18n.resolvedLanguage ?? i18n.language) as UiLanguage;

  const setUiLanguage = (next: UiLanguage) => {
    void i18n.changeLanguage(next);
  };

  return { uiLanguage, setUiLanguage, uiLanguages: UI_LANGUAGES };
}
