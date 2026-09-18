import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import { Close } from "@mui/icons-material";
import { DateTimePicker } from "@mui/x-date-pickers";
import { api, type ContentSummaryDto } from "../api/client";
import dayjs from "../lib/dayjs";

interface VersionHistoryProps {
  id: number;
  language: string;
  activeVersion?: number;
  onSelectVersion: (versionNumber: number) => void;
}

export default function VersionHistory({ id, language, activeVersion, onSelectVersion }: VersionHistoryProps) {
  const queryClient = useQueryClient();
  const { data: history = [] } = useQuery({
    queryKey: ["content-history", id, language],
    queryFn: () => api.getHistory(id, language),
  });

  const [publishTarget, setPublishTarget] = useState<ContentSummaryDto | undefined>(undefined);
  const [startPublish, setStartPublish] = useState<string | null>(null);
  const [stopPublish, setStopPublish] = useState<string | null>(null);

  async function invalidateAfterPublishChange() {
    await queryClient.invalidateQueries({ queryKey: ["update-schema", id, language] });
    await queryClient.invalidateQueries({ queryKey: ["content-history", id, language] });
    await queryClient.invalidateQueries({ queryKey: ["content-search"] });
  }

  function openPublishDialog(version: ContentSummaryDto) {
    setPublishTarget(version);
    setStartPublish(null);
    setStopPublish(null);
  }

  async function confirmPublish() {
    if (!publishTarget) return;
    await api.publishContent(id, { versionNumber: publishTarget.versionNumber, startPublish, stopPublish });
    setPublishTarget(undefined);
    await invalidateAfterPublishChange();
  }

  async function handleUnpublish() {
    await api.unpublishContent(id);
    await invalidateAfterPublishChange();
  }

  if (history.length === 0) {
    return (
      <Typography color="text.secondary" sx={{ px: 3, pb: 3 }}>
        No versions in this language yet.
      </Typography>
    );
  }

  return (
    <>
      <TableContainer sx={{ pb: 2 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Version</TableCell>
              <TableCell>Created</TableCell>
              <TableCell>Status</TableCell>
              <TableCell />
            </TableRow>
          </TableHead>
          <TableBody>
            {history.map((version) => {
              const isLive = version.versionNumber === version.livePublishedVersionNumber;
              const isScheduled = !isLive && !!version.startPublish && dayjs(version.startPublish).isAfter(dayjs());

              return (
                <TableRow
                  key={version.versionNumber}
                  hover
                  selected={version.versionNumber === activeVersion}
                  onClick={() => onSelectVersion(version.versionNumber)}
                  sx={{ cursor: "pointer" }}
                >
                  <TableCell>v{version.versionNumber}</TableCell>
                  <TableCell>{dayjs(version.createdAtUtc).format("YYYY-MM-DD HH:mm")}</TableCell>
                  <TableCell>
                    {isLive ? (
                      <Chip size="small" color="success" label="Published" />
                    ) : isScheduled ? (
                      <Chip size="small" color="info" label="Scheduled" />
                    ) : null}
                  </TableCell>
                  <TableCell align="right">
                    {isLive ? (
                      <Button
                        size="small"
                        onClick={(e) => {
                          e.stopPropagation();
                          handleUnpublish();
                        }}
                      >
                        Unpublish
                      </Button>
                    ) : (
                      <Button
                        size="small"
                        onClick={(e) => {
                          e.stopPropagation();
                          openPublishDialog(version);
                        }}
                      >
                        Publish
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={!!publishTarget} onClose={() => setPublishTarget(undefined)} maxWidth="xs" fullWidth>
        <DialogTitle>
          <Typography variant="h6">Publish v{publishTarget?.versionNumber}</Typography>
          <IconButton aria-label="close" onClick={() => setPublishTarget(undefined)} sx={{ position: "absolute", right: 8, top: 8 }}>
            <Close />
          </IconButton>
        </DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <DateTimePicker
              label="Publish at"
              ampm={false}
              value={startPublish ? dayjs.utc(startPublish).local() : null}
              onChange={(v) => setStartPublish(v ? v.utc().toISOString() : null)}
              slotProps={{ textField: { fullWidth: true, helperText: "Leave empty to publish immediately" } }}
            />
            <DateTimePicker
              label="Unpublish at"
              ampm={false}
              value={stopPublish ? dayjs.utc(stopPublish).local() : null}
              onChange={(v) => setStopPublish(v ? v.utc().toISOString() : null)}
              slotProps={{ textField: { fullWidth: true, helperText: "Leave empty for no scheduled end" } }}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPublishTarget(undefined)}>Cancel</Button>
          <Button variant="contained" onClick={confirmPublish}>
            Publish
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
