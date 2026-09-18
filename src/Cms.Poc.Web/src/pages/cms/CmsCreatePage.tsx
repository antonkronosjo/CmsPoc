import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Paper, Stack, TextField, Typography } from "@mui/material";
import { api, type CreateContentSchema } from "../../api/client";
import { useLanguage } from "../../context/LanguageContext";
import ContentForm from "../../forms/ContentForm";
import ContentTypePicker from "../../forms/ContentTypePicker";

export default function CmsCreatePage() {
  const { language } = useLanguage();
  const [contentTypeName, setContentTypeName] = useState("");

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Create content</Typography>
      <Paper sx={{ p: 2 }}>
        <ContentTypePicker value={contentTypeName} onChange={setContentTypeName} />
        {contentTypeName && <CreateForm key={contentTypeName + language} contentTypeName={contentTypeName} language={language} />}
      </Paper>
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
          navigate(`/cms/edit/${created.metadata.id}`);
        }}
      />
    </Stack>
  );
}
