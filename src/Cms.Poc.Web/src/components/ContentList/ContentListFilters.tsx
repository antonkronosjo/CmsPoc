import { Card, Stack, ToggleButton, ToggleButtonGroup, Typography } from "@mui/material";
import { DatePicker } from "@mui/x-date-pickers";
import { useTranslation } from "react-i18next";
import dayjs from "../../lib/dayjs";
import type { ContentListFilterField, ContentListFilterValues } from "./filterTypes";

interface ContentListFiltersProps {
  fields: ContentListFilterField[];
  value: ContentListFilterValues;
  onChange: (value: ContentListFilterValues) => void;
}

/// Renders the filter controls described by `fields` and reports changes as a
/// single combined value - the caller (ContentList) turns that into search options.
export default function ContentListFilters({ fields, value, onChange }: ContentListFiltersProps) {
  const { t } = useTranslation();

  return (
    // A Card (glass or solid paper, per the theme) behind the controls keeps their labels and
    // inputs readable over the frosted-glass gradient backdrop.
    <Card sx={{ p: 2 }}>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={3} sx={{ flexWrap: "wrap" }}>
        {fields.map((field) => {
          if (field.kind === "contentType") {
            return (
              <Stack key={field.label} spacing={0.5}>
                <Typography variant="caption" color="text.secondary">
                  {field.label}
                </Typography>
                <ToggleButtonGroup
                  value={value.contentTypeKeys}
                  onChange={(_, next: string[]) => onChange({ ...value, contentTypeKeys: next })}
                  size="small"
                  aria-label={field.label}
                >
                  {field.options.map((option) => (
                    <ToggleButton key={option.value} value={option.value}>
                      {option.label}
                    </ToggleButton>
                  ))}
                </ToggleButtonGroup>
              </Stack>
            );
          }

          return (
            <Stack key={field.label} spacing={0.5}>
              <Typography variant="caption" color="text.secondary">
                {field.label}
              </Typography>
              <Stack direction="row" spacing={1}>
                <DatePicker
                  label={t("contentListFilters.from")}
                  value={value.dateFrom ? dayjs(value.dateFrom) : null}
                  onChange={(v) => onChange({ ...value, dateFrom: v ? v.startOf("day").utc().toISOString() : null })}
                  slotProps={{ textField: { size: "small" }, field: { clearable: true } }}
                />
                <DatePicker
                  label={t("contentListFilters.to")}
                  value={value.dateTo ? dayjs(value.dateTo) : null}
                  onChange={(v) => onChange({ ...value, dateTo: v ? v.endOf("day").utc().toISOString() : null })}
                  slotProps={{ textField: { size: "small" }, field: { clearable: true } }}
                />
              </Stack>
            </Stack>
          );
        })}
      </Stack>
    </Card>
  );
}
