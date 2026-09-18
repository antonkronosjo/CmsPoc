import { Chip, Tooltip, type ChipProps } from "@mui/material";
import { Cancel, CheckCircle, FiberManualRecord, Schedule } from "@mui/icons-material";
import { getPublishStatus, type PublishStatusInput } from "../lib/publishStatus";

const STATUS_ICONS = {
  success: { icon: CheckCircle, color: "success" },
  info: { icon: Schedule, color: "info" },
  warning: { icon: Cancel, color: "warning" },
  default: { icon: FiberManualRecord, color: "disabled" },
} as const;

interface StatusIndicatorProps extends Omit<ChipProps, "color" | "label" | "variant"> {
  metadata: PublishStatusInput;
  variant?: "chip" | "icon";
}

export default function StatusIndicator({ metadata, variant = "chip", sx, ...chipProps }: StatusIndicatorProps) {
  const status = getPublishStatus(metadata);

  if (variant === "icon") {
    const { icon: Icon, color } = STATUS_ICONS[status.color];
    return (
      <Tooltip title={status.label}>
        <Icon fontSize="small" color={color} />
      </Tooltip>
    );
  }

  return (
    <Chip
      size="small"
      {...chipProps}
      color={status.color}
      label={status.label}
      sx={[
        {
          height: "auto",
          "& .MuiChip-label": { px: "0.6em", py: "0.2em", lineHeight: 1.4 },
        },
        ...(Array.isArray(sx) ? sx : [sx]),
      ]}
    />
  );
}
