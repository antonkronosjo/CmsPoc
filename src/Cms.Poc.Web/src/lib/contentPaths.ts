/// Public URL segment for each content type's page (e.g. /news/12). Keep in
/// sync with the routes in routes.tsx.
export const contentPathSegments: Record<string, string> = {
  EventContent: "event",
  NewsContent: "news",
  SpecialNewsContent: "specialnews",
};

/// The public page URL for a content item in a language (e.g. /sv/news/12), or null if its type has no page.
export function contentPath(contentTypeKey: string, id: number, language: string): string | null {
  const segment = contentPathSegments[contentTypeKey];
  return segment ? `/${language}/${segment}/${id}` : null;
}
