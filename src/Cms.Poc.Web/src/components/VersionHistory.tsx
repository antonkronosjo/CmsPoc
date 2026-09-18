import { useQuery } from "@tanstack/react-query";
import { Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from "@mui/material";
import dayjs from "dayjs";
import { api } from "../api/client";

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

  if (history.length === 0) {
    return (
      <Typography color="text.secondary" sx={{ px: 3, pb: 3 }}>
        No versions in this language yet.
      </Typography>
    );
  }

  return (
    <TableContainer sx={{ pb: 2 }}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Version</TableCell>
            <TableCell>Created</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {history.map((version) => (
            <TableRow
              key={version.versionNumber}
              hover
              selected={version.versionNumber === activeVersion}
              onClick={() => onSelectVersion(version.versionNumber)}
              sx={{ cursor: "pointer" }}
            >
              <TableCell>v{version.versionNumber}</TableCell>
              <TableCell>{dayjs(version.createdAtUtc).format("YYYY-MM-DD HH:mm")}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
