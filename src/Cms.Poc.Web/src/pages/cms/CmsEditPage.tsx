import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Alert, Box, Stack, TextField, Typography } from "@mui/material";
import { api, type UpdateContentSchema } from "../../api/client";
import { useLanguage } from "../../context/LanguageContext";
import ContentForm from "../../forms/ContentForm";
import VersionHistory from "../../components/VersionHistory";

export default function CmsEditPage() {
  const { contentId, versionId } = useParams();
  const { language } = useLanguage();
  const navigate = useNavigate();

  const id = Number(contentId);
  const version = versionId ? Number(versionId) : undefined;

  return (
    <EditPanel
      key={`${id}-${version}-${language}`}
      id={id}
      version={version}
      language={language}
      onSelectVersion={(v) => navigate(`/cms/edit/${id}/${v}`)}
      onSaved={() => navigate(`/cms/edit/${id}`)}
    />
  );
}

function EditPanel({
  id,
  version,
  language,
  onSelectVersion,
  onSaved,
}: {
  id: number;
  version: number | undefined;
  language: string;
  onSelectVersion: (versionNumber: number) => void;
  onSaved: () => void;
}) {
  const queryClient = useQueryClient();
  const queryKey = ["update-schema", id, language, version];
  const { data: schema, isLoading } = useQuery({ queryKey, queryFn: () => api.getUpdateSchema(id, language, version) });
  const [draft, setDraft] = useState<UpdateContentSchema | undefined>(undefined);
  const active = draft ?? schema;

  if (isLoading || !active) return <Typography color="text.secondary">Loading…</Typography>;

  const isNewLanguageBranch = active.metadata.versionNumber === 0;
  const isViewingHistoricalVersion = version !== undefined && version !== active.metadata.versionNumber;

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="subtitle2" color="text.secondary" gutterBottom>
          #{active.metadata.id} · {active.metadata.contentTypeName} ·{" "}
          {isNewLanguageBranch ? "no translation yet in this language" : `v${active.metadata.versionNumber}`}
        </Typography>
        {isViewingHistoricalVersion && (
          <Alert severity="info" sx={{ mb: 2 }}>
            You're viewing historical version {version}. Saving will create a new version based on this data.
          </Alert>
        )}
        <Stack spacing={2}>
          <TextField
            label="Name"
            variant="filled"
            fullWidth
            value={active.metadata.name}
            onChange={(e) => setDraft({ ...active, metadata: { ...active.metadata, name: e.target.value } })}
          />
          <ContentForm
            contentTypeName={active.metadata.contentTypeName}
            language={active.metadata.language}
            properties={active.properties}
            submitText={isNewLanguageBranch ? `Add ${language} translation` : "Save (creates a new version)"}
            onChange={(key, value) =>
              setDraft({ ...active, properties: { ...active.properties, [key]: { ...active.properties[key], value } } })
            }
            onSubmit={async () => {
              const updated = await api.updateContent(id, active);
              setDraft(undefined);
              queryClient.setQueryData(["update-schema", id, language, undefined], updated);
              queryClient.invalidateQueries({ queryKey: ["content-history", id, language] });
              queryClient.invalidateQueries({ queryKey: ["content-search"] });
              onSaved();
            }}
          />
        </Stack>
      </Box>
      <Box>
        <Typography variant="h6" gutterBottom>
          Version history ({language})
        </Typography>
        <VersionHistory id={id} language={language} activeVersion={active.metadata.versionNumber} onSelectVersion={onSelectVersion} />
      </Box>
    </Stack>
  );
}
