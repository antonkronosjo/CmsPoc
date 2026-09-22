import { useState } from "react";
import { Link as RouterLink } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import {
  Box,
  Chip,
  MenuItem,
  Pagination,
  Select,
  Skeleton,
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
import dayjs from "../lib/dayjs";
import { api, type ContentSummaryDto } from "../api/client";
import { getBranchPublishStatus, publishStatusColorHex } from "../lib/publishStatus";
import { chipColorSx } from "../lib/chipColor";
import { useContentTypes } from "../hooks/useContentTypes";
import { useDebouncedCallback } from "../hooks/useDebouncedCallback";
import ContentTypeChip from "./ContentTypeChip";

const PAGE_SIZE_OPTIONS = [10, 25, 50];

interface ContentBrowseListProps {
  /// Restricts the list to items translated into this language and shows them in it. Leave
  /// undefined to list every item in its master language, with a column of its languages.
  language?: string;
  onSelect?: (item: ContentSummaryDto) => void;
  showTypeFilter?: boolean;
  /// When true, only content with a version currently live is returned (the
  /// public view). When false (the default), every item is shown, using its
  /// published version's data when one exists and its latest draft otherwise.
  publishedOnly?: boolean;
}

/// Paginated, filterable content list - the shared table behind both the
/// home page (published-only) and the /cms browse page (everything, with a
/// type filter and a publish status badge).
export default function ContentBrowseList({ language, onSelect, showTypeFilter = false, publishedOnly = false }: ContentBrowseListProps) {
  const [input, setInput] = useState("");
  const [term, setTerm] = useState("");
  const [contentTypeName, setContentTypeName] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(PAGE_SIZE_OPTIONS[0]);
  const debouncedSetTerm = useDebouncedCallback((value: string) => {
    setTerm(value);
    setPage(1);
  }, 250);

  const { data: contentTypes = [] } = useContentTypes();

  const { data, isFetching } = useQuery({
    queryKey: ["content-search", term, language, contentTypeName, page, pageSize, publishedOnly],
    queryFn: () => api.searchContent(term, language, { contentTypeKey: contentTypeName || undefined, page, pageSize, publishedOnly }),
  });

  const items = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;
  const pageCount = data ? Math.max(1, Math.ceil(data.totalCount / pageSize)) : 1;
  const columnCount = 3 + (language === undefined ? 3 : 1) + (language !== undefined && !publishedOnly ? 1 : 0);
  const rangeStart = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const rangeEnd = Math.min(page * pageSize, totalCount);

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
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
              <MenuItem key={t.key} value={t.key}>
                {t.key}
              </MenuItem>
            ))}
          </Select>
        )}
      </Stack>

      {isFetching && data && (
        <Typography variant="caption" color="text.secondary">
          Loading…
        </Typography>
      )}

      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              {language === undefined && <TableCell>Id</TableCell>}
              <TableCell>Name</TableCell>
              <TableCell>Content Type</TableCell>
              {language === undefined && <TableCell>Languages</TableCell>}
              {language !== undefined && <TableCell>Version</TableCell>}
              <TableCell>Created</TableCell>
              {language === undefined && <TableCell>Last Modified</TableCell>}
              {language !== undefined && !publishedOnly && <TableCell>Status</TableCell>}
            </TableRow>
          </TableHead>
          <TableBody>
            {isFetching && !data
              ? Array.from({ length: 5 }).map((_, i) => (
                  <TableRow key={i}>
                    {Array.from({ length: columnCount }).map((_, j) => (
                      <TableCell key={j}>
                        <Skeleton variant="text" />
                      </TableCell>
                    ))}
                  </TableRow>
                ))
              : items.map((item) => (
              <TableRow
                key={item.id}
                hover={!!onSelect}
                onClick={onSelect ? () => onSelect(item) : undefined}
                sx={onSelect ? { cursor: "pointer" } : undefined}
              >
                {language === undefined && <TableCell>{item.id}</TableCell>}
                <TableCell>{item.name || <em>(untitled)</em>}</TableCell>
                <TableCell>
                  <ContentTypeChip contentTypeKey={item.contentTypeKey} />
                </TableCell>
                {language === undefined && (
                  <TableCell>
                    <Box sx={{ display: "flex", alignItems: "center", gap: 0.5 }} onClick={(e) => e.stopPropagation()}>
                      {(() => {
                        const others = item.languages.filter((l) => l !== item.masterLanguage).sort((a, b) => a.localeCompare(b));
                        const ordered = item.languages.includes(item.masterLanguage) ? [item.masterLanguage, ...others] : others;
                        return ordered.map((l, index) => {
                          const languageStatus = item.languageStatuses.find((s) => s.language === l);
                          const status = getBranchPublishStatus(
                            languageStatus ?? { startPublish: null, livePublishedVersionNumber: null, hasBeenPublished: false },
                          );
                          return (
                            <Box key={l} sx={{ display: "flex", alignItems: "center", gap: 0.5 }}>
                              {index === 1 && (
                                <Box component="span" sx={{ color: "text.disabled" }}>
                                  ·
                                </Box>
                              )}
                              <Chip
                                component={RouterLink}
                                to={`/cms/edit/${item.id}?lang=${l}`}
                                clickable={false}
                                size="small"
                                label={l}
                                title={`${status.label}${l === item.masterLanguage ? " · Master language" : ""}`}
                                sx={(theme) => chipColorSx(publishStatusColorHex(theme, status.color))}
                              />
                            </Box>
                          );
                        });
                      })()}
                    </Box>
                  </TableCell>
                )}
                {language !== undefined && <TableCell>v{item.versionNumber}</TableCell>}
                <TableCell>
                  {dayjs
                    .utc(language === undefined ? item.rootCreated : item.created)
                    .local()
                    .format("YYYY-MM-DD HH:mm")}
                </TableCell>
                {language === undefined && (
                  <TableCell>{dayjs.utc(item.lastModified).local().format("YYYY-MM-DD HH:mm")}</TableCell>
                )}
                {language !== undefined && !publishedOnly && (
                  <TableCell>
                    {item.livePublishedVersionNumber != null ? (
                      <Chip size="small" color="success" label="Published" />
                    ) : (
                      <Chip size="small" color="default" label="Draft" />
                    )}
                  </TableCell>
                )}
              </TableRow>
            ))}
          </TableBody>
        </Table>
        {!isFetching && data && items.length === 0 && (
          <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
            No content found.
          </Typography>
        )}
      </TableContainer>

      {data && totalCount > 0 && (
        <Stack direction={{ xs: "column", sm: "row" }} spacing={1} sx={{ alignItems: "center", justifyContent: "space-between" }}>
          <Typography variant="caption" color="text.secondary">
            Showing {rangeStart}–{rangeEnd} of {totalCount}
          </Typography>
          <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
            <Select
              size="small"
              value={pageSize}
              onChange={(e) => {
                setPageSize(Number(e.target.value));
                setPage(1);
              }}
              sx={{ minWidth: 110 }}
            >
              {PAGE_SIZE_OPTIONS.map((size) => (
                <MenuItem key={size} value={size}>
                  {size} / page
                </MenuItem>
              ))}
            </Select>
            {pageCount > 1 && <Pagination count={pageCount} page={page} onChange={(_, value) => setPage(value)} />}
          </Stack>
        </Stack>
      )}
    </Stack>
  );
}
