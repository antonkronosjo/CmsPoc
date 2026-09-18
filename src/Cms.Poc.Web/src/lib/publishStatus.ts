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
