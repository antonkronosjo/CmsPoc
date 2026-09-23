import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { Link as RouterLink } from "react-router-dom";
import { Link, Stack, Typography, type TypographyProps } from "@mui/material";
import { api } from "../../api/client";
import { useLanguage } from "../../hooks/useLanguage";
import ContentArea from "../ContentArea/ContentArea";

/// Items with no explicit `count` are effectively unlimited (landing pages
/// listing "all" of a type) - this just bounds that query at a sane size.
const UNLIMITED_PAGE_SIZE = 200;

interface ContentListProps {
  /// Heading shown above the grid.
  title: string;
  /// Restrict to one content type (e.g. "NewsContent"), several (e.g. every
  /// news variant), or omit for every type.
  contentTypeKey?: string | string[];
  /// How many of the most recently created items to show. Omit to show everything.
  count?: number;
  noContentText: string;
  /// Typography props for the heading - larger/h1 for a dedicated landing page,
  /// smaller/h2 (the default) for a section within a page that has its own h1.
  titleVariant?: TypographyProps["variant"];
  titleComponent?: TypographyProps["component"];
  /// Optional "see all" link shown next to the heading, e.g. linking a home page
  /// section to its dedicated landing page. `to` is relative to the current language.
  link?: { to: string; label: string };
}

/// A content section: heading, then a grid of the most recently created
/// published items, optionally filtered to one or more content types.
export default function ContentList({
  title,
  contentTypeKey,
  count,
  noContentText,
  titleVariant = "h5",
  titleComponent = "h2",
  link,
}: ContentListProps) {
  const { language } = useLanguage();
  const pageSize = count ?? UNLIMITED_PAGE_SIZE;
  const contentTypeKeys = Array.isArray(contentTypeKey) ? contentTypeKey : undefined;
  const singleContentTypeKey = Array.isArray(contentTypeKey) ? undefined : contentTypeKey;

  const { data, isFetching } = useQuery({
    queryKey: ["content-search", contentTypeKey ?? "", language, pageSize],
    queryFn: () =>
      api.searchContent("", language, {
        contentTypeKey: singleContentTypeKey,
        contentTypeKeys,
        pageSize,
        publishedOnly: true,
        sortBy: "rootCreated",
        sortDescending: true,
      }),
    gcTime: 0,
    placeholderData: keepPreviousData,
  });
  const items = data?.items ?? [];

  return (
    <Stack spacing={2} component="section">
      <Stack direction="row" spacing={2} sx={{ alignItems: "baseline", justifyContent: "space-between" }}>
        <Typography variant={titleVariant} component={titleComponent}>
          {title}
        </Typography>
        {link && (
          <Link component={RouterLink} to={`/${language}/${link.to}`} variant="body2" underline="hover">
            {link.label}
          </Link>
        )}
      </Stack>
      <ContentArea content={items} />
      {!isFetching && items.length === 0 && (
        <Typography variant="body2" color="text.secondary">
          {noContentText}
        </Typography>
      )}
    </Stack>
  );
}
