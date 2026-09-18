import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Box, Container, Grid, MenuItem, Paper, Select, Stack, TextField, Typography } from "@mui/material";
import { api, type CreateContentSchema, type UpdateContentSchema } from "./api/client";
import ContentForm from "./forms/ContentForm";
import ContentSearchList from "./components/ContentSearchList";
import VersionHistory from "./components/VersionHistory";

const LANGUAGES = ["en", "sv"];

function App() {
  const [language, setLanguage] = useState("en");
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [createType, setCreateType] = useState<string>("");
  const queryClient = useQueryClient();

  const { data: contentTypes = [] } = useQuery({ queryKey: ["content-types"], queryFn: api.getContentTypes });

  const refreshLists = () => {
    queryClient.invalidateQueries({ queryKey: ["content-search"] });
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={1} sx={{ mb: 3 }}>
        <Typography variant="h4" component="h1">
          Content Framework POC
        </Typography>
        <Typography color="text.secondary">
          Flat content models, generated persistence, immutable versioning - edited through one generic, schema-driven
          form that works for every registered content type.
        </Typography>
        <Select value={language} onChange={(e) => setLanguage(e.target.value)} size="small" sx={{ width: 160 }}>
          {LANGUAGES.map((l) => (
            <MenuItem key={l} value={l}>
              {l}
            </MenuItem>
          ))}
        </Select>
      </Stack>

      <Grid container spacing={2}>
        <Grid size={12}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Create content
            </Typography>
            <Select
              displayEmpty
              value={createType}
              onChange={(e) => setCreateType(e.target.value)}
              size="small"
              sx={{ minWidth: 220, mb: 2 }}
            >
              <MenuItem value="">
                <em>Choose a content type</em>
              </MenuItem>
              {contentTypes.map((t) => (
                <MenuItem key={t} value={t}>
                  {t}
                </MenuItem>
              ))}
            </Select>
            {createType && (
              <CreatePanel
                key={createType + language}
                contentTypeName={createType}
                language={language}
                onCreated={(created) => {
                  refreshLists();
                  setSelectedId(created.metadata.id);
                  setCreateType("");
                }}
              />
            )}
          </Paper>
        </Grid>

        <Grid size={{ xs: 12, md: 5 }}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Search
            </Typography>
            <ContentSearchList language={language} onSelect={(item) => setSelectedId(item.id)} />
          </Paper>
        </Grid>

        <Grid size={{ xs: 12, md: 7 }}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Edit
            </Typography>
            {selectedId ? (
              <EditPanel key={`${selectedId}-${language}`} id={selectedId} language={language} onSaved={refreshLists} />
            ) : (
              <Typography color="text.secondary">Select a search result, or create new content above.</Typography>
            )}
          </Paper>
        </Grid>
      </Grid>
    </Container>
  );
}

function CreatePanel({
  contentTypeName,
  language,
  onCreated,
}: {
  contentTypeName: string;
  language: string;
  onCreated: (created: UpdateContentSchema) => void;
}) {
  const { data: schema, isLoading } = useQuery({
    queryKey: ["creation-schema", contentTypeName, language],
    queryFn: () => api.getCreationSchema(contentTypeName, language),
  });
  const [draft, setDraft] = useState<CreateContentSchema | undefined>(undefined);
  const active = draft ?? schema;

  if (isLoading || !active) return <Typography color="text.secondary">Loading…</Typography>;

  return (
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
        submitText="Create"
        onChange={(key, value) =>
          setDraft({ ...active, properties: { ...active.properties, [key]: { ...active.properties[key], value } } })
        }
        onSubmit={async () => {
          const created = await api.createContent(active);
          setDraft(undefined);
          onCreated(created);
        }}
      />
    </Stack>
  );
}

function EditPanel({ id, language, onSaved }: { id: number; language: string; onSaved: () => void }) {
  const queryClient = useQueryClient();
  const queryKey = ["update-schema", id, language];
  const { data: schema, isLoading } = useQuery({ queryKey, queryFn: () => api.getUpdateSchema(id, language) });
  const [draft, setDraft] = useState<UpdateContentSchema | undefined>(undefined);
  const active = draft ?? schema;

  if (isLoading || !active) return <Typography color="text.secondary">Loading…</Typography>;

  const isNewLanguageBranch = active.metadata.versionNumber === 0;

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="subtitle2" color="text.secondary" gutterBottom>
          #{active.metadata.id} · {active.metadata.contentTypeName} ·{" "}
          {isNewLanguageBranch ? "no translation yet in this language" : `v${active.metadata.versionNumber}`}
        </Typography>
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
              queryClient.setQueryData(queryKey, updated);
              queryClient.invalidateQueries({ queryKey: ["content-history", id, language] });
              onSaved();
            }}
          />
        </Stack>
      </Box>
      <Box>
        <Typography variant="h6" gutterBottom>
          Version history ({language})
        </Typography>
        <VersionHistory id={id} language={language} />
      </Box>
    </Stack>
  );
}

export default App;
