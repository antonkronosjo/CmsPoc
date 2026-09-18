import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  Box,
  Chip,
  MenuItem,
  Pagination,
  Select,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from "@mui/material";
import { api, type ContentSummaryDto } from "../api/client";
import { useDebouncedCallback } from "../hooks/useDebouncedCallback";

const PAGE_SIZE = 10;

interface ContentBrowseListProps {
  language: string;
  onSelect: (item: ContentSummaryDto) => void;
  showTypeFilter?: boolean;
}

/// Paginated, filterable content list - the shared table behind both the
/// home page (unfiltered) and the /cms browse page (with a type filter).
export default function ContentBrowseList({ language, onSelect, showTypeFilter = false }: ContentBrowseListProps) {
  const [input, setInput] = useState("");
  const [term, setTerm] = useState("");
  const [contentTypeName, setContentTypeName] = useState("");
  const [page, setPage] = useState(1);
  const debouncedSetTerm = useDebouncedCallback((value: string) => {
    setTerm(value);
    setPage(1);
  }, 250);

  const { data: contentTypes = [] } = useQuery({ queryKey: ["content-types"], queryFn: api.getContentTypes });

  const { data, isFetching } = useQuery({
    queryKey: ["content-search", term, language, contentTypeName, page],
    queryFn: () => api.searchContent(term, language, { contentTypeName: contentTypeName || undefined, page, pageSize: PAGE_SIZE }),
  });

  const items = data?.items ?? [];
  const pageCount = data ? Math.max(1, Math.ceil(data.totalCount / PAGE_SIZE)) : 1;

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
        <TextField
          label="Search by name"
          variant="filled"
          size="small"
          fullWidth
          value={input}
          onChange={(e) => {
            setInput(e.target.value);
            debouncedSetTerm(e.target.value);
          }}
        />
        {showTypeFilter && (
          <Select
            displayEmpty
            size="small"
            value={contentTypeName}
            onChange={(e) => {
              setContentTypeName(e.target.value);
              setPage(1);
            }}
            sx={{ minWidth: 200 }}
          >
            <MenuItem value="">
              <em>All content types</em>
            </MenuItem>
            {contentTypes.map((t) => (
              <MenuItem key={t} value={t}>
                {t}
              </MenuItem>
            ))}
          </Select>
        )}
      </Stack>

      {isFetching && (
        <Typography variant="caption" color="text.secondary">
          Loading…
        </Typography>
      )}

      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>Version</TableCell>
              <TableCell>Created (UTC)</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {items.map((item) => (
              <TableRow key={item.id} hover onClick={() => onSelect(item)} sx={{ cursor: "pointer" }}>
                <TableCell>{item.name || <em>(untitled)</em>}</TableCell>
                <TableCell>
                  <Chip size="small" label={item.contentTypeName} />
                </TableCell>
                <TableCell>v{item.versionNumber}</TableCell>
                <TableCell>{new Date(item.createdAtUtc).toLocaleString()}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        {!isFetching && items.length === 0 && (
          <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
            No content found.
          </Typography>
        )}
      </TableContainer>

      {pageCount > 1 && (
        <Box sx={{ display: "flex", justifyContent: "center" }}>
          <Pagination count={pageCount} page={page} onChange={(_, value) => setPage(value)} />
        </Box>
      )}
    </Stack>
  );
}
