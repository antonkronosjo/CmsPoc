import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { Box, Button, Drawer, IconButton, Stack, Typography } from "@mui/material";
import { Close } from "@mui/icons-material";
import { DateTimePicker } from "@mui/x-date-pickers";
import { api } from "../api/client";
import dayjs from "../lib/dayjs";
import ConfirmDialog, { useConfirmDialog } from "./ConfirmDialog";

interface PublishTarget {
  versionNumber: number;
  currentLiveVersionNumber?: number | null;
}

interface UsePublishActionsOptions {
  language: string;
  id: number;
}

export function usePublishActions({ id, language }: UsePublishActionsOptions) {
  const queryClient = useQueryClient();
  const [publishTarget, setPublishTarget] = useState<PublishTarget | undefined>(undefined);
  const [startPublish, setStartPublish] = useState<string | null>(null);
  const [stopPublish, setStopPublish] = useState<string | null>(null);
  const { confirm, confirmDialogProps } = useConfirmDialog();

  async function invalidateAfterPublishChange() {
    // Publish state is per language branch, so only this language's views go stale.
    await queryClient.invalidateQueries({ queryKey: ["update-schema", id] });
    await queryClient.invalidateQueries({ queryKey: ["content-history", id] });
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
    const { versionNumber, currentLiveVersionNumber } = publishTarget;
    if (currentLiveVersionNumber != null && versionNumber < currentLiveVersionNumber) {
      const ok = await confirm({
        title: "Publish an older version?",
        message: `Version ${versionNumber} is older than the currently published version ${currentLiveVersionNumber}. Publishing it will replace the live content with this older version. Are you sure you want to continue?`,
        confirmText: "Publish anyway",
        confirmColor: "warning",
      });
      if (!ok) return;
    }
    await api.publishContent(id, { language, versionNumber, startPublish, stopPublish });
    setPublishTarget(undefined);
    await invalidateAfterPublishChange();
  }

  async function unpublish() {
    const ok = await confirm({
      title: "Unpublish content?",
      message: "This will take the currently live version offline immediately. Are you sure you want to continue?",
      confirmText: "Unpublish",
      confirmColor: "warning",
    });
    if (!ok) return;
    await api.unpublishContent(id, language);
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
    confirmDialogProps,
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
  confirmDialogProps,
}: PublishActions) {
  return (
    <>
      <Drawer anchor="right" open={!!publishTarget} onClose={closePublishDialog}>
        <Box sx={{ width: { xs: "85vw", sm: 380 }, display: "flex", flexDirection: "column", height: "100%" }}>
          <Stack direction="row" sx={{ alignItems: "center", justifyContent: "space-between", p: 3, pb: 2 }}>
            <Typography variant="h6">Publish v{publishTarget?.versionNumber}</Typography>
            <IconButton aria-label="close" onClick={closePublishDialog}>
              <Close />
            </IconButton>
          </Stack>
          <Box sx={{ borderBottom: "1px dotted", borderColor: "divider" }} />
          <Stack spacing={2} sx={{ p: 3, flex: 1 }}>
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
          <Box sx={{ borderBottom: "1px dotted", borderColor: "divider" }} />
          <Stack direction="row" spacing={1} sx={{ p: 2, justifyContent: "flex-end" }}>
            <Button onClick={closePublishDialog}>Cancel</Button>
            <Button variant="contained" onClick={confirmPublish}>
              Publish
            </Button>
          </Stack>
        </Box>
      </Drawer>
      <ConfirmDialog {...confirmDialogProps} />
    </>
  );
}
