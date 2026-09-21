import { useLocation, useNavigate } from "react-router-dom";
import { useLanguages } from "./useLanguages";

/// The public site's language. It lives in the URL as the first path segment
/// (/sv/news/12), so it is linkable and survives reloads. The CMS ignores it
/// and works with every language at once.
export function useLanguage() {
  const { pathname, search } = useLocation();
  const navigate = useNavigate();
  const { defaultLanguage, supportedLanguages } = useLanguages();

  const segments = pathname.split("/");
  const inUrl = supportedLanguages.includes(segments[1]);
  const language = inUrl ? segments[1] : defaultLanguage;

  /// Switches to another language on the same page by rewriting the URL's language segment.
  const setLanguage = (next: string) => {
    if (inUrl) segments[1] = next;
    else segments.splice(1, 0, next);
    navigate({ pathname: segments.join("/"), search });
  };

  return { language, setLanguage, languages: supportedLanguages };
}
