import { useQuery } from "@tanstack/react-query";
import { Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from "@mui/material";
import { api } from "../api/client";

interface VersionHistoryProps {
  id: number;
  language: string;
}

export default function VersionHistory({ id, language }: VersionHistoryProps) {
  const { data: history = [] } = useQuery({
    queryKey: ["content-history", id, language],
    queryFn: () => api.getHistory(id, language),
  });

  if (history.length === 0) {
    return <Typography color="text.secondary">No versions in this language yet.</Typography>;
  }

  return (
    <TableContainer>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Version</TableCell>
            <TableCell>Created (UTC)</TableCell>
            <TableCell>Fields</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {history.map((version) => (
            <TableRow key={version.versionNumber}>
              <TableCell>v{version.versionNumber}</TableCell>
              <TableCell>{new Date(version.createdAtUtc).toLocaleString()}</TableCell>
              <TableCell>
                {Object.entries(version.properties)
                  .map(([key, value]) => `${key}: ${value ?? "—"}`)
                  .join(" · ")}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
