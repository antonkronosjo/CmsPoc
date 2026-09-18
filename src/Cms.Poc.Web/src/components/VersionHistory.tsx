import { useQuery } from "@tanstack/react-query";
import { Button, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Tooltip, Typography } from "@mui/material";
import { Cancel, CheckCircle, FiberManualRecord, Schedule } from "@mui/icons-material";
import { api } from "../api/client";
import { getPublishStatus } from "../lib/publishStatus";
import PublishDialog, { usePublishActions } from "./PublishDialog";
import dayjs from "../lib/dayjs";

const STATUS_ICONS = {
  success: { icon: CheckCircle, color: "success" },
  info: { icon: Schedule, color: "info" },
  warning: { icon: Cancel, color: "warning" },
  default: { icon: FiberManualRecord, color: "disabled" },
} as const;

interface VersionHistoryProps {
  id: number;
  language: string;
  activeVersion?: number;
  onSelectVersion: (versionNumber: number) => void;
}

export default function VersionHistory({ id, language, activeVersion, onSelectVersion }: VersionHistoryProps) {
  const { data: history = [] } = useQuery({
    queryKey: ["content-history", id, language],
    queryFn: () => api.getHistory(id, language),
  });

  const publishActions = usePublishActions({ id, language });
  const { openPublishDialog, unpublish } = publishActions;

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
              <TableCell>Status</TableCell>
              <TableCell>Version</TableCell>
              <TableCell>Created</TableCell>
              <TableCell />
            </TableRow>
          </TableHead>
          <TableBody>
            {history.map((version) => {
              const isLive = version.versionNumber === version.livePublishedVersionNumber;
              const status = getPublishStatus(version);
              const { icon: StatusIcon, color: iconColor } = STATUS_ICONS[status.color];

              return (
                <TableRow
                  key={version.versionNumber}
                  hover
                  selected={version.versionNumber === activeVersion}
                  onClick={() => onSelectVersion(version.versionNumber)}
                  sx={{ cursor: "pointer" }}
                >
                  <TableCell>
                    <Tooltip title={status.label}>
                      <StatusIcon fontSize="small" color={iconColor} />
                    </Tooltip>
                  </TableCell>
                  <TableCell>v{version.versionNumber}</TableCell>
                  <TableCell>{dayjs(version.createdAtUtc).format("YYYY-MM-DD HH:mm")}</TableCell>
                  <TableCell align="right">
                    {isLive ? (
                      <Button
                        size="small"
                        onClick={(e) => {
                          e.stopPropagation();
                          unpublish();
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

      <PublishDialog {...publishActions} />
    </>
  );
}
