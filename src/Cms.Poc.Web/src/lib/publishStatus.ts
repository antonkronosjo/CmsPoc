import type { Theme } from "@mui/material/styles";
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
  return color === "default" ? theme.palette.grey[500] : theme.palette[color].main;
}

/// This exact version's own status: whether it is the one currently live, scheduled to
/// become live, previously live but not anymore, or never published.
export function getPublishStatus(metadata: PublishStatusInput): PublishStatus {
  if (metadata.versionNumber === metadata.livePublishedVersionNumber) {
    return { label: "Published", color: "success" };
  }
  if (metadata.startPublish && dayjs(metadata.startPublish).isAfter(dayjs())) {
    return { label: "Scheduled", color: "info" };
  }
  if (metadata.startPublish) {
    return { label: "Unpublished", color: "warning" };
  }
  return { label: "Draft", color: "default" };
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
export function getBranchPublishStatus(input: BranchStatusInput): PublishStatus {
  if (input.livePublishedVersionNumber != null) {
    return { label: "Published", color: "success" };
  }
  if (input.startPublish && dayjs(input.startPublish).isAfter(dayjs())) {
    return { label: "Scheduled", color: "info" };
  }
  if (input.hasBeenPublished) {
    return { label: "Unpublished", color: "warning" };
  }
  return { label: "Draft", color: "default" };
}
