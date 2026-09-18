import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Alert, Box, Button, Card, Chip, Drawer, Stack, TextField, Typography } from "@mui/material";
import { History } from "@mui/icons-material";
import dayjs from "dayjs";
import { api, type UpdateContentMetadata, type UpdateContentSchema } from "../../api/client";
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

function getPublishStatus(metadata: UpdateContentMetadata): { label: string; color: "success" | "warning" | "default" | "info" } {
  if (metadata.startPublish && dayjs(metadata.startPublish).isAfter(dayjs())) {
    return { label: `Scheduled to publish ${dayjs(metadata.startPublish).format("YYYY-MM-DD HH:mm")}`, color: "info" };
  }
  if (metadata.livePublishedVersionNumber == null) {
    return { label: "Draft", color: "default" };
  }
  if (metadata.livePublishedVersionNumber === metadata.versionNumber) {
    return { label: "Published", color: "success" };
  }
  return { label: `Published v${metadata.livePublishedVersionNumber}`, color: "warning" };
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
  const [historyOpen, setHistoryOpen] = useState(false);
  const active = draft ?? schema;

  if (isLoading || !active) return <Typography color="text.secondary">Loading…</Typography>;

  const isNewLanguageBranch = active.metadata.versionNumber === 0;
  const isViewingHistoricalVersion = version !== undefined && version !== active.metadata.versionNumber;

  return (
    <>
      <Card sx={{ p: 3 }}>
        <Box>
          <Typography variant="h5">{active.metadata.name || "(untitled)"}</Typography>
          <Stack direction="row" alignItems="center" sx={{ mt: 0.5 }}>
            <Typography variant="subtitle2" color="text.secondary">
              #{active.metadata.id} · {active.metadata.contentTypeName} ·
            </Typography>
            {isNewLanguageBranch ? (
              <Typography variant="subtitle2" color="text.secondary" sx={{ ml: 0.5 }}>
                no translation yet in this language
              </Typography>
            ) : (
              <>
                <Button
                  size="small"
                  variant="text"
                  color="primary"
                  endIcon={<History fontSize="small" />}
                  onClick={() => setHistoryOpen(true)}
                  sx={{
                    minWidth: 0,
                    ml: 0.5,
                    py: 0,
                    px: 0.75,
                    fontSize: "inherit",
                    fontWeight: 600,
                    lineHeight: "inherit",
                    textTransform: "none",
                    "& .MuiButton-endIcon": { ml: 0 },
                  }}
                >
                  v{active.metadata.versionNumber}
                </Button>
                <Chip size="small" sx={{ ml: 1 }} color={getPublishStatus(active.metadata).color} label={getPublishStatus(active.metadata).label} />
              </>
            )}
          </Stack>
        </Box>
        <Box sx={{ borderBottom: "1px dotted", borderColor: "divider", my: 2.5 }} />
        {isViewingHistoricalVersion && (
          <Alert severity="info" sx={{ mb: 2 }}>
            You're viewing historical version {version}. Saving will create a new version based on this data.
          </Alert>
        )}
        <Stack spacing={2}>
          <TextField
            label="Name"
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
      </Card>
      <Drawer anchor="right" open={historyOpen} onClose={() => setHistoryOpen(false)}>
        <Box sx={{ width: { xs: "85vw", sm: 380 } }}>
          <Typography variant="h6" sx={{ p: 3, pb: 2 }}>
            Version history ({language})
          </Typography>
          <Box sx={{ borderBottom: "1px dotted", borderColor: "divider" }} />
          <VersionHistory
            id={id}
            language={language}
            activeVersion={active.metadata.versionNumber}
            onSelectVersion={(v) => {
              setHistoryOpen(false);
              onSelectVersion(v);
            }}
          />
        </Box>
      </Drawer>
    </>
  );
}
