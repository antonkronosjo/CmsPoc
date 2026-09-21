import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Box, List, ListItemButton, ListItemText, Stack, TextField, Typography } from "@mui/material";
import { api, type ContentSummaryDto } from "../api/client";
import { useDebouncedCallback } from "../hooks/useDebouncedCallback";
import ContentTypeChip from "./ContentTypeChip";

interface ContentSearchListProps {
  language: string;
  onSelect: (item: ContentSummaryDto) => void;
  excludeId?: number;
}

/// Searchable list of content, shared by the main search panel and the
/// ContentPicker template - one place implements "find content by name."
export default function ContentSearchList({ language, onSelect, excludeId }: ContentSearchListProps) {
  const [input, setInput] = useState("");
  const [term, setTerm] = useState("");
  const debouncedSetTerm = useDebouncedCallback(setTerm, 250);

  const { data, isFetching } = useQuery({
    queryKey: ["content-search", term, language],
    queryFn: () => api.searchContent(term, language, { pageSize: 50 }),
  });
  const results = data?.items ?? [];

  const filtered = excludeId ? results.filter((r) => r.id !== excludeId) : results;

  return (
    <Stack spacing={1}>
      <TextField
        label="Search by name"
        size="small"
        fullWidth
        value={input}
        onChange={(e) => {
          setInput(e.target.value);
          debouncedSetTerm(e.target.value);
        }}
      />
      {isFetching && <Typography variant="caption" color="text.secondary">Searching…</Typography>}
      <List dense disablePadding sx={{ maxHeight: 360, overflowY: "auto" }}>
        {filtered.map((item) => (
          <ListItemButton key={item.id} onClick={() => onSelect(item)}>
            <ListItemText
              primary={
                <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                  <ContentTypeChip contentTypeKey={item.contentTypeKey} />
                  <span>{item.name || <em>(untitled)</em>}</span>
                </Box>
              }
              secondary={`#${item.id} · v${item.versionNumber} · ${item.language}`}
            />
          </ListItemButton>
        ))}
        {!isFetching && filtered.length === 0 && (
          <Typography variant="body2" color="text.secondary" sx={{ py: 1 }}>
            No content found.
          </Typography>
        )}
      </List>
    </Stack>
  );
}
