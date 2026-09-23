import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import en from "./locales/en.json";
import sv from "./locales/sv.json";

export const UI_LANGUAGE_STORAGE_KEY = "cms-poc-ui-language";
export const UI_LANGUAGES = ["en", "sv"] as const;
export type UiLanguage = (typeof UI_LANGUAGES)[number];

/// The CMS editor's chosen UI language. Only the CMS reads it - the public site's UI
/// language always follows its content language (see AppLayout), and is never persisted.
export function getStoredUiLanguage(): UiLanguage {
  const stored = localStorage.getItem(UI_LANGUAGE_STORAGE_KEY);
  return stored === "sv" ? "sv" : "en";
}

export function storeUiLanguage(language: UiLanguage) {
  localStorage.setItem(UI_LANGUAGE_STORAGE_KEY, language);
}

// Any content language may be passed to changeLanguage, including ones with no
// translation file - their static strings then fall back to English.
i18n.use(initReactI18next).init({
  resources: {
    en: { translation: en },
    sv: { translation: sv },
  },
  lng: getStoredUiLanguage(),
  fallbackLng: "en",
  interpolation: { escapeValue: false },
});

export default i18n;
