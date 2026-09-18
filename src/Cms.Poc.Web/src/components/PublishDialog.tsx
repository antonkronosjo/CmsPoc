import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { Button, Dialog, DialogActions, DialogContent, DialogTitle, IconButton, Stack, Typography } from "@mui/material";
import { Close } from "@mui/icons-material";
import { DateTimePicker } from "@mui/x-date-pickers";
import { api } from "../api/client";
import dayjs from "../lib/dayjs";

interface PublishTarget {
  versionNumber: number;
}

interface UsePublishActionsOptions {
  id: number;
  language: string;
}

export function usePublishActions({ id, language }: UsePublishActionsOptions) {
  const queryClient = useQueryClient();
  const [publishTarget, setPublishTarget] = useState<PublishTarget | undefined>(undefined);
  const [startPublish, setStartPublish] = useState<string | null>(null);
  const [stopPublish, setStopPublish] = useState<string | null>(null);

  async function invalidateAfterPublishChange() {
    await queryClient.invalidateQueries({ queryKey: ["update-schema", id, language] });
    await queryClient.invalidateQueries({ queryKey: ["content-history", id, language] });
    await queryClient.invalidateQueries({ queryKey: ["content-search"] });
  }

  function openPublishDialog(target: PublishTarget) {
    setPublishTarget(target);
    setStartPublish(dayjs().utc().toISOString());
    setStopPublish(null);
  }

  function closePublishDialog() {
    setPublishTarget(undefined);
  }

  async function confirmPublish() {
    if (!publishTarget) return;
    await api.publishContent(id, { versionNumber: publishTarget.versionNumber, startPublish, stopPublish });
    setPublishTarget(undefined);
    await invalidateAfterPublishChange();
  }

  async function unpublish() {
    await api.unpublishContent(id);
    await invalidateAfterPublishChange();
  }

  return {
    publishTarget,
    startPublish,
    stopPublish,
    setStartPublish,
    setStopPublish,
    openPublishDialog,
    closePublishDialog,
    confirmPublish,
    unpublish,
  };
}

export type PublishActions = ReturnType<typeof usePublishActions>;

export default function PublishDialog({
  publishTarget,
  startPublish,
  stopPublish,
  setStartPublish,
  setStopPublish,
  closePublishDialog,
  confirmPublish,
}: PublishActions) {
  return (
    <Dialog open={!!publishTarget} onClose={closePublishDialog} maxWidth="xs" fullWidth>
      <DialogTitle>
        <Typography variant="h6">Publish v{publishTarget?.versionNumber}</Typography>
        <IconButton aria-label="close" onClick={closePublishDialog} sx={{ position: "absolute", right: 8, top: 8 }}>
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
            slotProps={{ textField: { fullWidth: true, helperText: "Defaults to now - clear to publish immediately" } }}
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
        <Button onClick={closePublishDialog}>Cancel</Button>
        <Button variant="contained" onClick={confirmPublish}>
          Publish
        </Button>
      </DialogActions>
    </Dialog>
  );
}
