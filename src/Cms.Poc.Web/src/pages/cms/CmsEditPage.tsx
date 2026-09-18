import { useState, type ReactNode } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Alert, Box, Button, Card, Drawer, Stack, TextField, Typography } from "@mui/material";
import { Edit, History } from "@mui/icons-material";
import dayjs from "../../lib/dayjs";
import { api, type UpdateContentSchema } from "../../api/client";
import { useLanguage } from "../../context/LanguageContext";
import ContentForm from "../../forms/ContentForm";
import VersionHistory from "../../components/VersionHistory";
import StatusIndicator from "../../components/StatusIndicator";
import PublishDialog, { usePublishActions } from "../../components/PublishDialog";

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

function formatDate(value: string | null): string {
  return value ? dayjs.utc(value).local().format("YYYY-MM-DD HH:mm") : "—";
}

function MetaItem({ label, value }: { label: string; value: ReactNode }) {
  return (
    <Box>
      <Typography
        variant="caption"
        color="text.secondary"
        sx={{ display: "block", textTransform: "uppercase", letterSpacing: 0.5, fontSize: "0.65rem", lineHeight: 1.6 }}
      >
        {label}
      </Typography>
      <Typography component="div" variant="body2" color="text.primary" sx={{ fontSize: "0.65rem" }}>
        {value}
      </Typography>
    </Box>
  );
}

function EditableName({ name, onChange }: { name: string; onChange: (name: string) => void }) {
  const [editing, setEditing] = useState(false);

  if (editing) {
    return (
      <TextField
        autoFocus
        variant="standard"
        value={name}
        onChange={(e) => onChange(e.target.value)}
        onBlur={() => setEditing(false)}
        onKeyDown={(e) => {
          if (e.key === "Enter") setEditing(false);
        }}
        sx={{ "& input": { fontSize: "1.5rem", fontWeight: 400 } }}
      />
    );
  }

  return (
    <Box
      onClick={() => setEditing(true)}
      sx={{ display: "flex", alignItems: "center", gap: 0.75, cursor: "pointer", "&:hover .name-edit-icon": { opacity: 1 } }}
    >
      <Typography variant="h5">{name || "(untitled)"}</Typography>
      <Edit fontSize="small" className="name-edit-icon" sx={{ opacity: 0, transition: "opacity 0.15s", color: "text.secondary" }} />
    </Box>
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
  const [historyOpen, setHistoryOpen] = useState(false);
  const publishActions = usePublishActions({ id, language });
  const active = draft ?? schema;

  if (isLoading || !active) return <Typography color="text.secondary">Loading…</Typography>;

  const isNewLanguageBranch = active.metadata.versionNumber === 0;
  const isViewingHistoricalVersion = version !== undefined && version !== active.metadata.versionNumber;
  const isLive = active.metadata.versionNumber === active.metadata.livePublishedVersionNumber;

  return (
    <>
      <Card sx={{ p: 3 }}>
        <Box
          sx={{
            bgcolor: (theme) => (theme.palette.mode === "dark" ? "rgba(255,255,255,0.04)" : "grey.50"),
            border: "1px solid",
            borderColor: "divider",
            borderRadius: 2,
            p: 2,
          }}
        >
          <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "flex-start" }}>
            <EditableName
              name={active.metadata.name}
              onChange={(name) => setDraft({ ...active, metadata: { ...active.metadata, name } })}
            />
            {!isNewLanguageBranch &&
              (isLive ? (
                <Button variant="outlined" color="warning" onClick={() => publishActions.unpublish()}>
                  Unpublish
                </Button>
              ) : (
                <Button
                  variant="contained"
                  onClick={() => publishActions.openPublishDialog({ versionNumber: active.metadata.versionNumber })}
                >
                  Publish
                </Button>
              ))}
          </Stack>
          {isNewLanguageBranch ? (
            <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 0.5 }}>
              no translation yet in this language
            </Typography>
          ) : (
            <Box
              sx={{
                display: "grid",
                gridTemplateColumns: "repeat(auto-fill, minmax(110px, max-content))",
                columnGap: 3,
                rowGap: 1.25,
                mt: 1.5,
              }}
            >
              <MetaItem label="Id" value={active.metadata.id} />
              <MetaItem label="Content type" value={active.metadata.contentTypeName} />
              <MetaItem
                label="Version"
                value={
                  <Button
                    size="small"
                    variant="text"
                    color="primary"
                    endIcon={<History fontSize="small" />}
                    onClick={() => setHistoryOpen(true)}
                    sx={{
                      minWidth: 0,
                      py: 0,
                      px: 0,
                      fontSize: "inherit",
                      fontWeight: 600,
                      lineHeight: "inherit",
                      textTransform: "none",
                      "& .MuiButton-endIcon": { ml: 0.5 },
                    }}
                  >
                    v{active.metadata.versionNumber}
                  </Button>
                }
              />
              <MetaItem label="Status" value={<StatusIndicator metadata={active.metadata} sx={{ fontSize: "inherit" }} />} />
              <MetaItem label="StartPublish" value={formatDate(active.metadata.startPublish)} />
              <MetaItem label="StopPublish" value={formatDate(active.metadata.stopPublish)} />
            </Box>
          )}
        </Box>
        <Box sx={{ borderBottom: "1px dotted", borderColor: "divider", my: 2.5 }} />
        {isViewingHistoricalVersion && (
          <Alert severity="info" sx={{ mb: 2 }}>
            You're viewing historical version {version}. Saving will create a new version based on this data.
          </Alert>
        )}
        <Stack spacing={2}>
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
      <PublishDialog {...publishActions} />
    </>
  );
}
