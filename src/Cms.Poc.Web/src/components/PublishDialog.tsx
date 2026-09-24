import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { Box, Button, CircularProgress, Drawer, IconButton, Stack, Typography } from "@mui/material";
import { Close } from "@mui/icons-material";
import { DateTimePicker } from "@mui/x-date-pickers";
import { useTranslation } from "react-i18next";
import { api } from "../api/client";
import dayjs from "../lib/dayjs";
import ConfirmDialog, { useConfirmDialog } from "./ConfirmDialog";
import { useToast, errorMessage } from "../context/ToastContext";

interface PublishTarget {
  versionNumber: number;
  currentLiveVersionNumber?: number | null;
  latestVersionNumber?: number | null;
}

interface UsePublishActionsOptions {
  language: string;
  id: number;
  /// Called with the version actually published - a new copy's number when the requested one had been live before.
  onPublished?: (versionNumber: number) => void;
}

export function usePublishActions({ id, language, onPublished }: UsePublishActionsOptions) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [publishTarget, setPublishTarget] = useState<PublishTarget | undefined>(undefined);
  const [startPublish, setStartPublish] = useState<string | null>(null);
  const [stopPublish, setStopPublish] = useState<string | null>(null);
  const [publishing, setPublishing] = useState(false);
  const [unpublishing, setUnpublishing] = useState(false);
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
    const { versionNumber, currentLiveVersionNumber, latestVersionNumber } = publishTarget;
    const isOlderThanLive = currentLiveVersionNumber != null && versionNumber < currentLiveVersionNumber;
    const isOlderThanLatest = latestVersionNumber != null && versionNumber < latestVersionNumber;
    if (isOlderThanLive || isOlderThanLatest) {
      const message =
        isOlderThanLive && isOlderThanLatest
          ? t("publishDialog.confirmOlderBoth", { versionNumber, currentLiveVersionNumber, latestVersionNumber })
          : isOlderThanLive
            ? t("publishDialog.confirmOlderThanLive", { versionNumber, currentLiveVersionNumber })
            : t("publishDialog.confirmOlderThanDraft", { versionNumber, latestVersionNumber });
      const ok = await confirm({
        title: t("publishDialog.publishOlderTitle"),
        message,
        confirmText: t("publishDialog.publishAnyway"),
        confirmColor: "warning",
      });
      if (!ok) return;
    }
    setPublishing(true);
    try {
      const published = await api.publishContent(id, { language, versionNumber, startPublish, stopPublish });
      setPublishTarget(undefined);
      await invalidateAfterPublishChange();
      showToast(
        published.versionNumber === versionNumber
          ? t("publishDialog.publishedToast", { versionNumber })
          : t("publishDialog.publishedAsCopyToast", { sourceVersionNumber: versionNumber, versionNumber: published.versionNumber }),
      );
      onPublished?.(published.versionNumber);
    } catch (error) {
      showToast(errorMessage(error, t("publishDialog.publishFailed")), "error");
    } finally {
      setPublishing(false);
    }
  }

  async function unpublish() {
    const ok = await confirm({
      title: t("publishDialog.unpublishTitle"),
      message: t("publishDialog.unpublishMessage"),
      confirmText: t("publishDialog.unpublish"),
      confirmColor: "warning",
    });
    if (!ok) return;
    setUnpublishing(true);
    try {
      await api.unpublishContent(id, language);
      await invalidateAfterPublishChange();
      showToast(t("publishDialog.unpublishedToast"));
    } catch (error) {
      showToast(errorMessage(error, t("publishDialog.unpublishFailed")), "error");
    } finally {
      setUnpublishing(false);
    }
  }

  return {
    publishTarget,
    startPublish,
    stopPublish,
    publishing,
    unpublishing,
    setStartPublish,
    setStopPublish,
    openPublishDialog,
    closePublishDialog,
    confirmPublish,
    unpublish,
    confirm,
    confirmDialogProps,
  };
}

export type PublishActions = ReturnType<typeof usePublishActions>;

export default function PublishDialog({
  publishTarget,
  startPublish,
  stopPublish,
  publishing,
  setStartPublish,
  setStopPublish,
  closePublishDialog,
  confirmPublish,
  confirmDialogProps,
}: PublishActions) {
  const { t } = useTranslation();
  return (
    <>
      <Drawer anchor="right" open={!!publishTarget} onClose={closePublishDialog}>
        <Box sx={{ width: { xs: "85vw", sm: 380 }, display: "flex", flexDirection: "column", height: "100%" }}>
          <Stack direction="row" sx={{ alignItems: "center", justifyContent: "space-between", p: 3, pb: 2 }}>
            <Typography variant="h6">{t("publishDialog.drawerHeading", { versionNumber: publishTarget?.versionNumber })}</Typography>
            <IconButton aria-label={t("common.close")} onClick={closePublishDialog}>
              <Close />
            </IconButton>
          </Stack>
          <Box sx={{ borderBottom: "1px dotted", borderColor: "divider" }} />
          <Stack spacing={2} sx={{ p: 3, flex: 1 }}>
            <DateTimePicker
              label={t("publishDialog.publishAtLabel")}
              ampm={false}
              format="YYYY-MM-DD HH:mm"
              value={startPublish ? dayjs.utc(startPublish).local() : null}
              onChange={(v) => setStartPublish(v ? v.utc().toISOString() : null)}
              slotProps={{ textField: { fullWidth: true, helperText: t("publishDialog.publishAtHelper") } }}
            />
            <DateTimePicker
              label={t("publishDialog.unpublishAtLabel")}
              ampm={false}
              format="YYYY-MM-DD HH:mm"
              value={stopPublish ? dayjs.utc(stopPublish).local() : null}
              onChange={(v) => setStopPublish(v ? v.utc().toISOString() : null)}
              slotProps={{ textField: { fullWidth: true, helperText: t("publishDialog.unpublishAtHelper") } }}
            />
          </Stack>
          <Box sx={{ borderBottom: "1px dotted", borderColor: "divider" }} />
          <Stack direction="row" spacing={1} sx={{ p: 2, justifyContent: "flex-end" }}>
            <Button onClick={closePublishDialog} disabled={publishing}>
              {t("common.cancel")}
            </Button>
            <Button
              variant="contained"
              onClick={confirmPublish}
              disabled={publishing}
              startIcon={publishing ? <CircularProgress size={16} color="inherit" /> : undefined}
            >
              {publishing ? t("publishDialog.publishing") : t("publishDialog.publish")}
            </Button>
          </Stack>
        </Box>
      </Drawer>
      <ConfirmDialog {...confirmDialogProps} />
    </>
  );
}
