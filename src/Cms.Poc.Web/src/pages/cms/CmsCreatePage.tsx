import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Card, MenuItem, Select, Stack, TextField, Typography } from "@mui/material";
import { api, type CreateContentSchema } from "../../api/client";
import { useLanguages } from "../../hooks/useLanguages";
import ContentForm from "../../forms/ContentForm";
import ContentTypePicker from "../../forms/ContentTypePicker";

export default function CmsCreatePage() {
  const { defaultLanguage, supportedLanguages } = useLanguages();
  const [contentTypeName, setContentTypeName] = useState("");
  // The language an item is created in becomes its master language.
  const [chosenLanguage, setChosenLanguage] = useState<string | null>(null);
  const language = chosenLanguage ?? defaultLanguage;

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Create content</Typography>
      <Card sx={{ p: 3 }}>
        <Stack direction="row" spacing={2} sx={{ alignItems: "flex-start" }}>
          <ContentTypePicker value={contentTypeName} onChange={setContentTypeName} />
          <Select
            value={language}
            onChange={(e) => setChosenLanguage(e.target.value)}
            size="small"
            sx={{ minWidth: 140, mb: 2 }}
            renderValue={(l) => `Master language: ${l}`}
          >
            {supportedLanguages.map((l) => (
              <MenuItem key={l} value={l}>
                {l}
              </MenuItem>
            ))}
          </Select>
        </Stack>
        {contentTypeName && <CreateForm key={contentTypeName + language} contentTypeName={contentTypeName} language={language} />}
      </Card>
    </Stack>
  );
}

function CreateForm({ contentTypeName, language }: { contentTypeName: string; language: string }) {
  const navigate = useNavigate();
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
        fullWidth
        value={active.metadata.name}
        onChange={(e) => setDraft({ ...active, metadata: { ...active.metadata, name: e.target.value } })}
      />
      <ContentForm
        contentTypeName={active.metadata.contentTypeKey}
        language={active.metadata.language}
        properties={active.properties}
        submitText="Create"
        onChange={(key, value) =>
          setDraft({ ...active, properties: { ...active.properties, [key]: { ...active.properties[key], value } } })
        }
        onSubmit={async () => {
          const created = await api.createContent(active);
          navigate(`/cms/edit/${created.metadata.id}`);
        }}
      />
    </Stack>
  );
}
