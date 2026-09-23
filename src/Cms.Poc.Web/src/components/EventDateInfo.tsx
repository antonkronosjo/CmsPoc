import { EventAvailable, EventBusy } from "@mui/icons-material";
import { Box, Chip, Paper, Stack, Typography, alpha } from "@mui/material";
import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import dayjs from "../lib/dayjs";

type EventStatus = "upcoming" | "ongoing" | "ended";

function getEventStatus(start: dayjs.Dayjs, end: dayjs.Dayjs): EventStatus {
  const now = dayjs();
  if (now.isBefore(start)) return "upcoming";
  if (now.isAfter(end)) return "ended";
  return "ongoing";
}

/** Compact one-line range for cards, e.g. "Mon 5 Oct 2026, 10:00–16:00" or "5 Oct 10:00 – 7 Oct 2026 16:00". */
export function formatEventRange(startDate: string, endDate?: string): string {
  const start = dayjs(startDate);
  if (!endDate) return start.format("ddd D MMM YYYY, HH:mm");
  const end = dayjs(endDate);
  if (start.isSame(end, "day")) return `${start.format("ddd D MMM YYYY, HH:mm")}–${end.format("HH:mm")}`;
  return `${start.format("D MMM HH:mm")} – ${end.format("D MMM YYYY HH:mm")}`;
}

function DateBlock({ icon, label, date }: { icon: ReactNode; label: string; date: dayjs.Dayjs }) {
  return (
    <Stack direction="row" spacing={1.5} sx={{ flex: 1, minWidth: 0 }}>
      <Box sx={{ color: "primary.main", display: "flex", pt: 0.25 }}>{icon}</Box>
      <Box sx={{ minWidth: 0 }}>
        <Typography variant="overline" color="text.secondary" sx={{ lineHeight: 1.5, display: "block" }}>
          {label}
        </Typography>
        <Typography variant="subtitle1" sx={{ fontWeight: 600, lineHeight: 1.3 }}>
          {date.format("dddd D MMMM YYYY")}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {date.format("HH:mm")}
        </Typography>
      </Box>
    </Stack>
  );
}

export default function EventDateInfo({ startDate, endDate }: { startDate: string; endDate?: string }) {
  const { t } = useTranslation();
  const start = dayjs(startDate);
  const end = endDate ? dayjs(endDate) : undefined;

  return (
    <Paper
      variant="outlined"
      sx={{
        p: { xs: 2, sm: 2.5 },
        borderRadius: 2,
        borderColor: (theme) => alpha(theme.palette.primary.main, 0.35),
        borderLeftWidth: 4,
        borderLeftColor: "primary.main",
        bgcolor: (theme) => alpha(theme.palette.primary.main, theme.palette.mode === "light" ? 0.06 : 0.12),
      }}
    >
      <Stack direction={{ xs: "column", sm: "row" }} spacing={{ xs: 2, sm: 3 }} sx={{ alignItems: { sm: "flex-start" } }}>
        <DateBlock icon={<EventAvailable />} label={t("event.starts")} date={start} />
        {end && <DateBlock icon={<EventBusy />} label={t("event.ends")} date={end} />}
      </Stack>
    </Paper>
  );
}

/** Upcoming / Ongoing / Ended tag, derived from the event's start and end dates. */
export function EventStatusChip({ startDate, endDate }: { startDate?: string; endDate?: string }) {
  const { t } = useTranslation();
  if (!startDate || !endDate) return null;
  const status = getEventStatus(dayjs(startDate), dayjs(endDate));
  return (
    <Chip
      size="small"
      label={t(`event.status.${status}`)}
      color={status === "ongoing" ? "success" : status === "upcoming" ? "primary" : "default"}
      variant={status === "ended" ? "outlined" : "filled"}
    />
  );
}
