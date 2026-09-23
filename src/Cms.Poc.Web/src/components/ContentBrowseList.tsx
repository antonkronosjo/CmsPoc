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
  TableSortLabel,
  TextField,
  Typography,
} from "@mui/material";
import { useTranslation } from "react-i18next";
import dayjs from "../lib/dayjs";
import { api, type ContentSummaryDto } from "../api/client";
import { getBranchPublishStatus, publishStatusColorHex } from "../lib/publishStatus";
import { chipColorSx } from "../lib/chipColor";
import { useContentTypes } from "../hooks/useContentTypes";
import { useDebouncedCallback } from "../hooks/useDebouncedCallback";

const PAGE_SIZE_OPTIONS = [10, 25, 50];

/// One column header with a discrete sort arrow, driven by the shared sortBy/sortDescending
/// state below - field is a ContentSummaryDto property name, sorted server-side (see
/// IContentEditingService.Search), not client-side.
function SortableHeaderCell({
  label,
  field,
  sortBy,
  sortDescending,
  onSort,
}: {
  label: string;
  field: string;
  sortBy: string | undefined;
  sortDescending: boolean;
  onSort: (field: string) => void;
}) {
  const active = sortBy === field;
  return (
    <TableCell sortDirection={active ? (sortDescending ? "desc" : "asc") : false}>
      <TableSortLabel active={active} direction={active && sortDescending ? "desc" : "asc"} onClick={() => onSort(field)}>
        {label}
      </TableSortLabel>
    </TableCell>
  );
}

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
  const { t } = useTranslation();
  const [input, setInput] = useState("");
  const [term, setTerm] = useState("");
  const [contentTypeName, setContentTypeName] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(PAGE_SIZE_OPTIONS[0]);
  const [sortBy, setSortBy] = useState<string | undefined>(undefined);
  const [sortDescending, setSortDescending] = useState(false);
  const debouncedSetTerm = useDebouncedCallback((value: string) => {
    setTerm(value);
    setPage(1);
  }, 250);

  const handleSort = (field: string) => {
    if (sortBy === field) {
      setSortDescending((prev) => !prev);
    } else {
      setSortBy(field);
      setSortDescending(false);
    }
    setPage(1);
  };

  const { data: contentTypes = [] } = useContentTypes();

  const { data, isFetching } = useQuery({
    queryKey: ["content-search", term, language, contentTypeName, page, pageSize, publishedOnly, sortBy, sortDescending],
    queryFn: () => api.searchContent(term, language, { contentTypeKey: contentTypeName || undefined, page, pageSize, publishedOnly, sortBy, sortDescending }),
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
          label={t("common.searchByName")}
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
              <em>{t("browseList.allContentTypes")}</em>
            </MenuItem>
            {contentTypes.map((contentType) => (
              <MenuItem key={contentType.key} value={contentType.key}>
                {contentType.key}
              </MenuItem>
            ))}
          </Select>
        )}
      </Stack>

      {isFetching && data && (
        <Typography variant="caption" color="text.secondary">
          {t("common.loading")}
        </Typography>
      )}

      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              {language === undefined && (
                <SortableHeaderCell label={t("browseList.columns.id")} field="id" sortBy={sortBy} sortDescending={sortDescending} onSort={handleSort} />
              )}
              <SortableHeaderCell label={t("browseList.columns.name")} field="name" sortBy={sortBy} sortDescending={sortDescending} onSort={handleSort} />
              <SortableHeaderCell
                label={t("browseList.columns.contentType")}
                field="contentTypeKey"
                sortBy={sortBy}
                sortDescending={sortDescending}
                onSort={handleSort}
              />
              {/* Languages is a list, not a single sortable value, so it gets a plain header. */}
              {language === undefined && <TableCell>{t("browseList.columns.languages")}</TableCell>}
              {language !== undefined && (
                <SortableHeaderCell
                  label={t("browseList.columns.version")}
                  field="versionNumber"
                  sortBy={sortBy}
                  sortDescending={sortDescending}
                  onSort={handleSort}
                />
              )}
              <SortableHeaderCell
                label={t("browseList.columns.created")}
                field={language === undefined ? "rootCreated" : "created"}
                sortBy={sortBy}
                sortDescending={sortDescending}
                onSort={handleSort}
              />
              {language === undefined && (
                <SortableHeaderCell
                  label={t("browseList.columns.lastModified")}
                  field="lastModified"
                  sortBy={sortBy}
                  sortDescending={sortDescending}
                  onSort={handleSort}
                />
              )}
              {language !== undefined && !publishedOnly && (
                <SortableHeaderCell
                  label={t("browseList.columns.status")}
                  field="livePublishedVersionNumber"
                  sortBy={sortBy}
                  sortDescending={sortDescending}
                  onSort={handleSort}
                />
              )}
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
                <TableCell>{item.name || <em>{t("common.untitled")}</em>}</TableCell>
                <TableCell>{item.contentTypeKey}</TableCell>
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
                            t,
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
                                title={`${status.label}${l === item.masterLanguage ? t("browseList.masterLanguageSuffix") : ""}`}
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
                      <Chip size="small" color="success" label={t("status.published")} />
                    ) : (
                      <Chip size="small" color="default" label={t("status.draft")} />
                    )}
                  </TableCell>
                )}
              </TableRow>
            ))}
          </TableBody>
        </Table>
        {!isFetching && data && items.length === 0 && (
          <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
            {t("common.noContentFound")}
          </Typography>
        )}
      </TableContainer>

      {data && totalCount > 0 && (
        <Stack direction={{ xs: "column", sm: "row" }} spacing={1} sx={{ alignItems: "center", justifyContent: "space-between" }}>
          <Typography variant="caption" color="text.secondary">
            {t("browseList.showingRange", { start: rangeStart, end: rangeEnd, total: totalCount })}
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
                  {t("browseList.perPage", { size })}
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
