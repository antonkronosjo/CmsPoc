import { useEffect, useState, type ReactNode } from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Alert, Box, Button, Card, Drawer, Menu, MenuItem, Stack, Tab, Tabs, TextField, Tooltip, Typography } from "@mui/material";
import { Add, Circle, Edit, History } from "@mui/icons-material";
import dayjs from "../../lib/dayjs";
import { api, type UpdateContentSchema } from "../../api/client";
import { useLanguages } from "../../hooks/useLanguages";
import ContentForm from "../../forms/ContentForm";
import VersionHistory from "../../components/VersionHistory";
import StatusIndicator from "../../components/StatusIndicator";
import ContentTypeChip from "../../components/ContentTypeChip";
import PublishDialog, { usePublishActions } from "../../components/PublishDialog";

/// Edits one content item with a tab per language, so every translation is
/// always in view. The item's master language comes first; shared
/// (non-culture-specific) fields and the name are only editable there.
/// The selected tab lives in the URL (?lang=sv) so it can be linked to.
export default function CmsEditPage() {
  const { contentId, versionId } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { supportedLanguages } = useLanguages();

  const id = Number(contentId);
  const version = versionId ? Number(versionId) : undefined;

  // Languages added in this session that have no saved translation yet (their tab stays open until saved).
  const [added, setAdded] = useState<string[]>([]);
  const [dirty, setDirty] = useState<ReadonlySet<string>>(new Set());
  const [addMenuAnchor, setAddMenuAnchor] = useState<HTMLElement | null>(null);

  // Which languages exist and which is the master, independent of any single language.
  const { data: summary, isError } = useQuery({
    queryKey: ["content-summary", id, "all"],
    queryFn: () => api.getContentSummary(id),
  });

  if (isError) return <Alert severity="error">Content {id} could not be loaded.</Alert>;
  if (!summary) return <Typography color="text.secondary">Loading…</Typography>;

  const master = summary.masterLanguage;
  const requested = searchParams.get("lang");
  const tabs = [
    ...new Set([
      master,
      ...summary.languages,
      ...added,
      ...(requested && supportedLanguages.includes(requested) ? [requested] : []),
    ]),
  ];
  const active = requested && tabs.includes(requested) ? requested : master;
  const addable = supportedLanguages.filter((l) => !tabs.includes(l));

  const selectLanguage = (language: string) => navigate({ pathname: `/cms/edit/${id}`, search: `?lang=${language}` });

  const setLanguageDirty = (language: string, isDirty: boolean) =>
    setDirty((previous) => {
      if (previous.has(language) === isDirty) return previous;
      const next = new Set(previous);
      if (isDirty) next.add(language);
      else next.delete(language);
      return next;
    });

  return (
    <Stack spacing={2} sx={{ flex: 1, minWidth: 0 }}>
      <Stack direction="row" sx={{ alignItems: "center", borderBottom: 1, borderColor: "divider" }}>
        <Tabs value={active} onChange={(_, language) => selectLanguage(language)} variant="scrollable" sx={{ flex: 1 }}>
          {tabs.map((language) => (
            <Tab
              key={language}
              value={language}
              label={
                <Stack direction="row" spacing={0.75} sx={{ alignItems: "center" }}>
                  <span>{language}</span>
                  {language === master && <Typography variant="caption" color="text.secondary">master</Typography>}
                  {!summary.languages.includes(language) && <Typography variant="caption" color="text.secondary">new</Typography>}
                  {dirty.has(language) && <Circle color="warning" sx={{ fontSize: 8 }} titleAccess="Unsaved changes" />}
                </Stack>
              }
            />
          ))}
        </Tabs>
        <Button startIcon={<Add />} disabled={addable.length === 0} onClick={(e) => setAddMenuAnchor(e.currentTarget)}>
          Add language
        </Button>
        <Menu anchorEl={addMenuAnchor} open={addMenuAnchor !== null} onClose={() => setAddMenuAnchor(null)}>
          {addable.map((language) => (
            <MenuItem
              key={language}
              onClick={() => {
                setAddMenuAnchor(null);
                setAdded((previous) => [...previous, language]);
                selectLanguage(language);
              }}
            >
              {language}
            </MenuItem>
          ))}
        </Menu>
      </Stack>

      {/* Every tab stays mounted, so unsaved edits survive switching languages. */}
      {tabs.map((language) => {
        const panelVersion = language === active ? version : undefined;
        return (
          <Box key={language} sx={{ display: language === active ? "block" : "none" }}>
            <EditPanel
              key={`${language}-${panelVersion}`}
              id={id}
              version={panelVersion}
              language={language}
              masterLanguage={master}
              onSelectVersion={(v) => navigate({ pathname: `/cms/edit/${id}/${v}`, search: `?lang=${language}` })}
              onSaved={() => selectLanguage(language)}
              onDirtyChange={(isDirty) => setLanguageDirty(language, isDirty)}
            />
          </Box>
        );
      })}
    </Stack>
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

function EditableName({ name, onChange, readOnly }: { name: string; onChange: (name: string) => void; readOnly?: boolean }) {
  const [editing, setEditing] = useState(false);

  if (readOnly) {
    return (
      <Tooltip title="The name is shared by all languages - edit it in the master language">
        <Typography variant="h5">{name || "(untitled)"}</Typography>
      </Tooltip>
    );
  }

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

/// True when the draft differs from the saved schema in something this language may edit.
/// Outside the master language only culture-specific values count: shared values are shown
/// read-only from the saved schema and ignored by the backend.
function hasEditableChanges(draft: UpdateContentSchema | undefined, schema: UpdateContentSchema | undefined, isMaster: boolean): boolean {
  if (!draft || !schema) return false;
  if (isMaster && draft.metadata.name !== schema.metadata.name) return true;
  return Object.entries(draft.properties).some(
    ([key, property]) =>
      (isMaster || property.cultureSpecific) && JSON.stringify(property.value) !== JSON.stringify(schema.properties[key]?.value),
  );
}

function EditPanel({
  id,
  version,
  language,
  masterLanguage,
  onSelectVersion,
  onSaved,
  onDirtyChange,
}: {
  id: number;
  version: number | undefined;
  language: string;
  masterLanguage: string;
  onSelectVersion: (versionNumber: number) => void;
  onSaved: () => void;
  onDirtyChange: (dirty: boolean) => void;
}) {
  const queryClient = useQueryClient();
  const queryKey = ["update-schema", id, language, version];
  const { data: schema, isLoading } = useQuery({ queryKey, queryFn: () => api.getUpdateSchema(id, language, version) });
  const [draft, setDraft] = useState<UpdateContentSchema | undefined>(undefined);
  const [historyOpen, setHistoryOpen] = useState(false);
  const publishActions = usePublishActions({ id });
  const active = draft ?? schema;
  const isMaster = language === masterLanguage;
  const hasChanges = hasEditableChanges(draft, schema, isMaster);

  useEffect(() => {
    onDirtyChange(hasChanges);
  }, [hasChanges, onDirtyChange]);

  if (isLoading || !active) return <Typography color="text.secondary">Loading…</Typography>;

  // Outside the master language shared values always show what is saved, never a stale draft of them.
  const displayProperties = Object.fromEntries(
    Object.entries(active.properties).map(([key, property]) => [key, isMaster || property.cultureSpecific ? property : (schema?.properties[key] ?? property)]),
  );

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
              readOnly={!isMaster}
              onChange={(name) => setDraft({ ...active, metadata: { ...active.metadata, name } })}
            />
            {!isNewLanguageBranch &&
              (isLive ? (
                <Tooltip title="Unpublishes the item in all languages">
                  <Button variant="outlined" color="warning" onClick={() => publishActions.unpublish()}>
                    Unpublish
                  </Button>
                </Tooltip>
              ) : (
                <Tooltip title={hasChanges ? "You have pending changes, save before publishing" : "Publishes this version in all languages"}>
                  <span>
                    <Button
                      variant="contained"
                      disabled={hasChanges}
                      onClick={() =>
                        publishActions.openPublishDialog({
                          versionNumber: active.metadata.versionNumber,
                          currentLiveVersionNumber: active.metadata.livePublishedVersionNumber,
                        })
                      }
                    >
                      Publish
                    </Button>
                  </span>
                </Tooltip>
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
              <MetaItem label="Content type" value={<ContentTypeChip contentTypeKey={active.metadata.contentTypeKey} />} />
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
            contentTypeName={active.metadata.contentTypeKey}
            language={language}
            masterLanguage={masterLanguage}
            properties={displayProperties}
            submitText={isNewLanguageBranch ? `Add ${language} translation` : "Save"}
            submitDisabled={!hasChanges}
            submitDisabledReason="No changes detected"
            onChange={(key, value) =>
              setDraft({ ...active, properties: { ...active.properties, [key]: { ...active.properties[key], value } } })
            }
            onSubmit={async () => {
              const updated = await api.updateContent(id, active);
              setDraft(undefined);
              queryClient.setQueryData(["update-schema", id, language, undefined], updated);
              // Every save is a new version of the whole item, so the other languages' tabs and the
              // list of translated languages are stale too.
              queryClient.invalidateQueries({ queryKey: ["update-schema", id], predicate: (q) => q.queryKey[2] !== language });
              queryClient.invalidateQueries({ queryKey: ["content-summary", id] });
              queryClient.invalidateQueries({ queryKey: ["content-history", id] });
              queryClient.invalidateQueries({ queryKey: ["content-search"] });
              onSaved();
            }}
          />
        </Stack>
      </Card>
      <Drawer anchor="right" open={historyOpen} onClose={() => setHistoryOpen(false)}>
        <Box sx={{ maxWidth: "100vw" }}>
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
