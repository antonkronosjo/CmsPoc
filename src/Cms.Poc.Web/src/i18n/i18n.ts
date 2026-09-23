import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import en from "./locales/en.json";
import sv from "./locales/sv.json";

export const UI_LANGUAGE_STORAGE_KEY = "cms-poc-ui-language";
export const UI_LANGUAGES = ["en", "sv"] as const;
export type UiLanguage = (typeof UI_LANGUAGES)[number];

function getInitialUiLanguage(): UiLanguage {
  const stored = localStorage.getItem(UI_LANGUAGE_STORAGE_KEY);
  return stored === "sv" ? "sv" : "en";
}

i18n.use(initReactI18next).init({
  resources: {
    en: { translation: en },
    sv: { translation: sv },
  },
  lng: getInitialUiLanguage(),
  fallbackLng: "en",
  interpolation: { escapeValue: false },
});

i18n.on("languageChanged", (lng) => {
  localStorage.setItem(UI_LANGUAGE_STORAGE_KEY, lng);
});

export default i18n;
