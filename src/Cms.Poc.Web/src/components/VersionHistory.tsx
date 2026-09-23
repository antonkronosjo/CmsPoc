import { useQuery } from "@tanstack/react-query";
import { Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import type { TFunction } from "i18next";
import { api, type UserRefDto } from "../api/client";
import StatusIndicator from "./StatusIndicator";
import PublishDialog, { usePublishActions } from "./PublishDialog";
import dayjs from "../lib/dayjs";

function userLabel(user: UserRefDto | null, t: TFunction) {
  if (!user) return "";
  return user.removed ? t("versionHistory.unknownUser") : (user.displayName ?? user.id);
}

interface VersionHistoryProps {
  id: number;
  language: string;
  activeVersion?: number;
  onSelectVersion: (versionNumber: number) => void;
}

export default function VersionHistory({ id, language, activeVersion, onSelectVersion }: VersionHistoryProps) {
  const { t } = useTranslation();
  const { data: history = [] } = useQuery({
    queryKey: ["content-history", id, language],
    queryFn: () => api.getHistory(id, language),
  });

  const publishActions = usePublishActions({ id, language });

  if (history.length === 0) {
    return (
      <Typography color="text.secondary" sx={{ px: 3, pb: 3 }}>
        {t("versionHistory.noVersions")}
      </Typography>
    );
  }

  return (
    <>
      <TableContainer sx={{ pb: 2 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>{t("versionHistory.columns.status")}</TableCell>
              <TableCell>{t("versionHistory.columns.version")}</TableCell>
              <TableCell>{t("versionHistory.columns.created")}</TableCell>
              <TableCell>{t("versionHistory.columns.createdBy")}</TableCell>
              <TableCell>{t("versionHistory.columns.publishedBy")}</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {history.map((version) => {
              return (
                <TableRow
                  key={version.versionNumber}
                  hover
                  selected={version.versionNumber === activeVersion}
                  onClick={() => onSelectVersion(version.versionNumber)}
                  sx={{ cursor: "pointer" }}
                >
                  <TableCell>
                    <StatusIndicator metadata={version} variant="icon" />
                  </TableCell>
                  <TableCell>v{version.versionNumber}</TableCell>
                  <TableCell>{dayjs(version.created).format("YYYY-MM-DD HH:mm")}</TableCell>
                  <TableCell>{userLabel(version.createdBy, t)}</TableCell>
                  <TableCell>{userLabel(version.publishedBy, t)}</TableCell>
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
