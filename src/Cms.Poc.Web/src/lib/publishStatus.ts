import type { Theme } from "@mui/material/styles";
import type { TFunction } from "i18next";
import dayjs from "./dayjs";

export interface PublishStatusInput {
  versionNumber: number;
  startPublish: string | null;
  livePublishedVersionNumber: number | null;
}

export interface PublishStatus {
  label: string;
  color: "success" | "warning" | "default" | "info";
}

/// Resolves a PublishStatus color to a literal hex value, for callers (like a gradient
/// chip fill) that need an actual color rather than a themed MUI component prop.
export function publishStatusColorHex(theme: Theme, color: PublishStatus["color"]): string {
  if (color === "default") {
    return theme.palette.mode === "light" ? theme.palette.grey[300] : theme.palette.grey[500];
  }
  return theme.palette[color].main;
}

/// This exact version's own status: whether it is the one currently live, scheduled to
/// become live, previously live but not anymore, or never published.
export function getPublishStatus(metadata: PublishStatusInput, t: TFunction): PublishStatus {
  if (metadata.versionNumber === metadata.livePublishedVersionNumber) {
    return { label: t("status.published"), color: "success" };
  }
  if (metadata.startPublish && dayjs(metadata.startPublish).isAfter(dayjs())) {
    return { label: t("status.scheduled"), color: "info" };
  }
  if (metadata.startPublish) {
    return { label: t("status.unpublished"), color: "warning" };
  }
  return { label: t("status.draft"), color: "default" };
}

export interface BranchStatusInput {
  /// The latest version's own start-publish, used only to detect an upcoming scheduled publish.
  startPublish: string | null;
  livePublishedVersionNumber: number | null;
  /// Whether ANY version of the branch, ever, has had a publish window set - not just the latest one's.
  hasBeenPublished: boolean;
}

/// A language branch's overall status: whether ANY version of it is currently live, about to
/// go live, was live before and has since been taken down - even if the newest version is a
/// fresh, never-published draft sitting on top of that history - or has never been published at all.
export function getBranchPublishStatus(input: BranchStatusInput, t: TFunction): PublishStatus {
  if (input.livePublishedVersionNumber != null) {
    return { label: t("status.published"), color: "success" };
  }
  if (input.startPublish && dayjs(input.startPublish).isAfter(dayjs())) {
    return { label: t("status.scheduled"), color: "info" };
  }
  if (input.hasBeenPublished) {
    return { label: t("status.unpublished"), color: "warning" };
  }
  return { label: t("status.draft"), color: "default" };
}
