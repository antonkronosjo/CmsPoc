import { useState } from "react";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { Box, List, ListItemButton, ListItemText, Skeleton, Stack, TextField, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { api, type ContentSummaryDto } from "../api/client";
import { useDebouncedCallback } from "../hooks/useDebouncedCallback";

interface ContentSearchListProps {
  /// Language used to fetch results; leave undefined to search every item in its master language.
  language?: string;
  onSelect: (item: ContentSummaryDto) => void;
  excludeId?: number;
}

/// Searchable list of content, shared by the main search panel and the
/// ContentPicker template - one place implements "find content by name."
export default function ContentSearchList({ language, onSelect, excludeId }: ContentSearchListProps) {
  const { t } = useTranslation();
  const [input, setInput] = useState("");
  const [term, setTerm] = useState("");
  const debouncedSetTerm = useDebouncedCallback(setTerm, 250);

  const { data, isFetching } = useQuery({
    queryKey: ["content-search", term, language],
    queryFn: () => api.searchContent(term, language, { pageSize: 50 }),
    // CMS content must read as current, not a stale snapshot - dropping the cache the moment
    // this view isn't shown anymore means coming back to it always re-fetches rather than
    // flashing whatever was last seen here.
    gcTime: 0,
    // Keep the previous results on screen while the new fetch above is in flight, instead of
    // clearing the list.
    placeholderData: keepPreviousData,
  });
  const results = data?.items ?? [];

  const filtered = excludeId ? results.filter((r) => r.id !== excludeId) : results;

  return (
    <Stack spacing={1}>
      <TextField
        label={t("common.searchByName")}
        size="small"
        fullWidth
        value={input}
        onChange={(e) => {
          setInput(e.target.value);
          debouncedSetTerm(e.target.value);
        }}
      />
      {isFetching && data && <Typography variant="caption" color="text.secondary">{t("common.searching")}</Typography>}
      <List dense disablePadding sx={{ maxHeight: 360, overflowY: "auto" }}>
        {isFetching && !data ? (
          Array.from({ length: 4 }).map((_, i) => (
            <ListItemButton key={i} disabled>
              <ListItemText primary={<Skeleton variant="text" width="60%" />} secondary={<Skeleton variant="text" width="40%" />} />
            </ListItemButton>
          ))
        ) : (
          <>
            {filtered.map((item) => (
              <ListItemButton key={item.id} onClick={() => onSelect(item)}>
                <ListItemText
                  primary={
                    <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                      <Typography variant="caption" color="text.secondary">
                        {item.contentTypeKey}
                      </Typography>
                      <span>{item.name || <em>{t("common.untitled")}</em>}</span>
                    </Box>
                  }
                  secondary={`#${item.id} · v${item.versionNumber} · ${item.language}`}
                />
              </ListItemButton>
            ))}
            {!isFetching && filtered.length === 0 && (
              <Typography variant="body2" color="text.secondary" sx={{ py: 1 }}>
                {t("common.noContentFound")}
              </Typography>
            )}
          </>
        )}
      </List>
    </Stack>
  );
}
