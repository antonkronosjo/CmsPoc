import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { Stack, Typography } from "@mui/material";
import { api } from "../../api/client";
import { useLanguage } from "../../hooks/useLanguage";
import ContentArea from "../ContentArea/ContentArea";

interface ContentListProps {
  /// Section heading shown above the grid.
  title: string;
  /// Restrict to one content type (e.g. "NewsContent"), or omit for every type.
  contentTypeKey?: string;
  /// How many of the most recently created items to show.
  count: number;
  noContentText: string;
}

/// A "latest N" section: title, then a grid of the most recently created
/// published items, optionally filtered to one content type.
export default function ContentList({ title, contentTypeKey, count, noContentText }: ContentListProps) {
  const { language } = useLanguage();

  const { data, isFetching } = useQuery({
    queryKey: ["content-search", contentTypeKey ?? "", language, count],
    queryFn: () =>
      api.searchContent("", language, {
        contentTypeKey,
        pageSize: count,
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
      <Typography variant="h5" component="h2">
        {title}
      </Typography>
      <ContentArea content={items} />
      {!isFetching && items.length === 0 && (
        <Typography variant="body2" color="text.secondary">
          {noContentText}
        </Typography>
      )}
    </Stack>
  );
}
