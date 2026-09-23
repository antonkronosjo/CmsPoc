/// Declarative description of one filter control. Which fields make sense
/// depends on the content type(s) being listed - a page assembles the set
/// that fits its own content (e.g. a content-type toggle plus a date range).
export type ContentListFilterField =
  | { kind: "contentType"; label: string; options: { value: string; label: string }[] }
  /// A date range. Omitting `property` filters the built-in publish date;
  /// giving one filters that named DateTime content property instead (e.g. "StartDate").
  | { kind: "dateRange"; label: string; property?: string };

export interface ContentListFilterValues {
  /// Selected content types; empty means no restriction (show every type the page lists).
  contentTypeKeys: string[];
  dateFrom: string | null;
  dateTo: string | null;
}

export const emptyContentListFilterValues: ContentListFilterValues = {
  contentTypeKeys: [],
  dateFrom: null,
  dateTo: null,
};
