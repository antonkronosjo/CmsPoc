import { useEffect, useRef, useState, type ReactNode } from "react";
import { useBlocker, useNavigate, useParams, useSearchParams, Link as RouterLink } from "react-router-dom";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Alert,
  Box,
  Breadcrumbs,
  Button,
  CircularProgress,
  Drawer,
  Fade,
  Menu,
  MenuItem,
  Skeleton,
  Stack,
  Tab,
  Tabs,
  TextField,
  Tooltip,
  Typography,
} from "@mui/material";
import { Add, Circle, Edit, History } from "@mui/icons-material";
import dayjs from "../../lib/dayjs";
import { api, type UpdateContentSchema } from "../../api/client";
import { useLanguages } from "../../hooks/useLanguages";
import ContentForm, { type ContentFormHandle } from "../../forms/ContentForm";
import VersionHistory from "../../components/VersionHistory";
import StatusIndicator from "../../components/StatusIndicator";
import ContentTypeChip from "../../components/ContentTypeChip";
import PublishDialog, { usePublishActions } from "../../components/PublishDialog";
import ConfirmDialog from "../../components/ConfirmDialog";
import { useToast, errorMessage } from "../../context/ToastContext";

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
  const hasUnsavedChanges = dirty.size > 0;

  // Which languages exist and which is the master, independent of any single language.
  const { data: summary, isError } = useQuery({
    queryKey: ["content-summary", id, "all"],
    queryFn: () => api.getContentSummary(id),
  });

  // Blocks in-app navigation away from the page while any language tab has unsaved
  // edits; the browser's own dialog covers closing the tab or reloading.
  const blocker = useBlocker(({ currentLocation, nextLocation }) => hasUnsavedChanges && currentLocation.pathname !== nextLocation.pathname);

  useEffect(() => {
    if (!hasUnsavedChanges) return;
    const onBeforeUnload = (e: BeforeUnloadEvent) => e.preventDefault();
    window.addEventListener("beforeunload", onBeforeUnload);
    return () => window.removeEventListener("beforeunload", onBeforeUnload);
  }, [hasUnsavedChanges]);

  const unsavedChangesGuard = (
    <ConfirmDialog
      open={blocker.state === "blocked"}
      title="Leave without saving?"
      message="You have unsaved changes that will be lost if you leave this page."
      confirmText="Leave"
      confirmColor="warning"
      onConfirm={() => blocker.state === "blocked" && blocker.proceed()}
      onCancel={() => blocker.state === "blocked" && blocker.reset()}
    />
  );

  if (isError)
    return (
      <Alert severity="error" action={<Button onClick={() => navigate("/cms")}>Back to browse</Button>}>
        Content {id} could not be loaded.
      </Alert>
    );
  if (!summary)
    return (
      <Stack spacing={2}>
        <Skeleton variant="text" width={160} height={40} />
        <Skeleton variant="rounded" height={320} />
      </Stack>
    );

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
      <Breadcrumbs>
        <Typography component={RouterLink} to="/cms" color="primary" sx={{ textDecoration: "none", fontWeight: 500, "&:hover": { textDecoration: "underline" } }}>
          Browse content
        </Typography>
        <Typography color="text.secondary">{summary.name || "(untitled)"}</Typography>
      </Breadcrumbs>
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
          <Fade key={language} in={language === active} timeout={150}>
            <Box sx={{ display: language === active ? "block" : "none" }}>
              <EditPanel
                key={`${language}-${panelVersion}`}
                id={id}
                version={panelVersion}
                language={language}
                masterLanguage={master}
                isActive={language === active}
                onSelectVersion={(v) => navigate({ pathname: `/cms/edit/${id}/${v}`, search: `?lang=${language}` })}
                onSaved={() => selectLanguage(language)}
                onDirtyChange={(isDirty) => setLanguageDirty(language, isDirty)}
              />
            </Box>
          </Fade>
        );
      })}
      {unsavedChangesGuard}
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

/// True when the draft differs from the saved schema in something this language may edit.
/// Name is culture-specific and always counts. Outside the master language, shared
/// (non-culture-specific) property values are shown read-only from the saved schema and
/// ignored by the backend.
function hasEditableChanges(draft: UpdateContentSchema | undefined, schema: UpdateContentSchema | undefined, isMaster: boolean): boolean {
  if (!draft || !schema) return false;
  if (draft.metadata.name !== schema.metadata.name) return true;
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
  isActive,
  onSelectVersion,
  onSaved,
  onDirtyChange,
}: {
  id: number;
  version: number | undefined;
  language: string;
  masterLanguage: string;
  isActive: boolean;
  onSelectVersion: (versionNumber: number) => void;
  onSaved: () => void;
  onDirtyChange: (dirty: boolean) => void;
}) {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const queryKey = ["update-schema", id, language, version];
  const { data: schema, isLoading } = useQuery({ queryKey, queryFn: () => api.getUpdateSchema(id, language, version) });
  const [draft, setDraft] = useState<UpdateContentSchema | undefined>(undefined);
  const [historyOpen, setHistoryOpen] = useState(false);
  const publishActions = usePublishActions({ id, language });
  const formRef = useRef<ContentFormHandle>(null);
  const active = draft ?? schema;
  const isMaster = language === masterLanguage;
  const hasChanges = hasEditableChanges(draft, schema, isMaster);

  useEffect(() => {
    onDirtyChange(hasChanges);
  }, [hasChanges, onDirtyChange]);

  // Ctrl/Cmd+S saves the currently visible tab, matching the platform save convention.
  useEffect(() => {
    if (!isActive) return;
    const onKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "s") {
        e.preventDefault();
        formRef.current?.submit();
      }
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [isActive]);

  if (isLoading || !active)
    return (
      <Box>
        <Skeleton variant="rounded" height={120} sx={{ mb: 2.5 }} />
        <Skeleton variant="rounded" height={56} sx={{ mb: 2 }} />
        <Skeleton variant="rounded" height={56} sx={{ mb: 2 }} />
        <Skeleton variant="rounded" height={100} />
      </Box>
    );

  // Outside the master language shared values always show what is saved, never a stale draft of them.
  const displayProperties = Object.fromEntries(
    Object.entries(active.properties).map(([key, property]) => [key, isMaster || property.cultureSpecific ? property : (schema?.properties[key] ?? property)]),
  );

  const isNewLanguageBranch = active.metadata.versionNumber === 0;
  const isLive = active.metadata.versionNumber === active.metadata.livePublishedVersionNumber;
  const isNotLatest =
    active.metadata.latestVersionNumber != null && active.metadata.versionNumber !== active.metadata.latestVersionNumber;

  return (
    <>
      <Box>
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
                <Tooltip title="Unpublishes this language only">
                  <span>
                    <Button
                      variant="outlined"
                      color="warning"
                      disabled={publishActions.unpublishing}
                      onClick={() => publishActions.unpublish()}
                      startIcon={publishActions.unpublishing ? <CircularProgress size={16} color="inherit" /> : undefined}
                    >
                      {publishActions.unpublishing ? "Unpublishing…" : "Unpublish"}
                    </Button>
                  </span>
                </Tooltip>
              ) : (
                <Tooltip title={hasChanges ? "You have pending changes, save before publishing" : "Publishes this version in this language only"}>
                  <span>
                    <Button
                      variant="contained"
                      disabled={hasChanges}
                      onClick={() =>
                        publishActions.openPublishDialog({
                          versionNumber: active.metadata.versionNumber,
                          currentLiveVersionNumber: active.metadata.livePublishedVersionNumber,
                          latestVersionNumber: active.metadata.latestVersionNumber,
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
        {isNotLatest && !isLive && (
          <Alert severity="info" sx={{ mb: 2 }}>
            You're viewing version {active.metadata.versionNumber}, not the latest (v{active.metadata.latestVersionNumber}). Saving
            will create a new version based on this data.
          </Alert>
        )}
        {isNotLatest && isLive && (
          <Alert severity="info" sx={{ mb: 2 }}>
            You're viewing version {active.metadata.versionNumber}, which is currently published, but a newer draft (v
            {active.metadata.latestVersionNumber}) exists.
          </Alert>
        )}
        <Stack spacing={2}>
          <ContentForm
            ref={formRef}
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
              // Only warn for a genuinely stale version - one that's neither the latest draft nor
              // the currently published version. Editing the published version is a normal path
              // (it's what the edit view loads by default), so it shouldn't be confirmed here.
              if (isNotLatest && !isLive) {
                const ok = await publishActions.confirm({
                  title: "Save over a newer version?",
                  message: `Version ${active.metadata.versionNumber} is not the latest (v${active.metadata.latestVersionNumber}). Saving now will create version ${active.metadata.latestVersionNumber! + 1} based on this older data - any changes made since v${active.metadata.versionNumber} won't be reflected. Continue?`,
                  confirmText: "Save anyway",
                  confirmColor: "warning",
                });
                if (!ok) return;
              }
              try {
                const updated = await api.updateContent(id, active);
                setDraft(undefined);
                queryClient.setQueryData(["update-schema", id, language, undefined], updated);
                // A save creates a new version in this language only, but a first save adds a language, so the
                // other tabs' list of languages is stale.
                queryClient.invalidateQueries({ queryKey: ["update-schema", id], predicate: (q) => q.queryKey[2] !== language });
                queryClient.invalidateQueries({ queryKey: ["content-summary", id] });
                queryClient.invalidateQueries({ queryKey: ["content-history", id] });
                queryClient.invalidateQueries({ queryKey: ["content-search"] });
                onSaved();
                showToast(isNewLanguageBranch ? `${language} translation added.` : "Saved.");
              } catch (error) {
                showToast(errorMessage(error, "Failed to save."), "error");
              }
            }}
          />
        </Stack>
      </Box>
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
