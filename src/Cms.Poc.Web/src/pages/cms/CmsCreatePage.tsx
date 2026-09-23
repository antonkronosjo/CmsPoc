import { useRef, useState } from "react";
import { useNavigate, Link as RouterLink } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Box, Breadcrumbs, MenuItem, Select, Skeleton, Stack, TextField, Typography } from "@mui/material";
import { NoteAdd } from "@mui/icons-material";
import { useTranslation } from "react-i18next";
import { api, type CreateContentSchema } from "../../api/client";
import { useLanguages } from "../../hooks/useLanguages";
import ContentForm, { type ContentFormHandle } from "../../forms/ContentForm";
import ContentTypePicker from "../../forms/ContentTypePicker";
import { useToast, errorMessage } from "../../context/ToastContext";

export default function CmsCreatePage() {
  const { t } = useTranslation();
  const { defaultLanguage, supportedLanguages } = useLanguages();
  const [contentTypeName, setContentTypeName] = useState("");
  // The language an item is created in becomes its master language.
  const [chosenLanguage, setChosenLanguage] = useState<string | null>(null);
  const language = chosenLanguage ?? defaultLanguage;

  return (
    <Stack spacing={2}>
      <Breadcrumbs>
        <Typography component={RouterLink} to="/cms" color="primary" sx={{ textDecoration: "none", fontWeight: 500, "&:hover": { textDecoration: "underline" } }}>
          {t("cmsCreatePage.breadcrumbBrowse")}
        </Typography>
        <Typography color="text.secondary">{t("cmsCreatePage.breadcrumbCreate")}</Typography>
      </Breadcrumbs>
      <Typography variant="h5">{t("cmsCreatePage.heading")}</Typography>
      <Stack spacing={2}>
        <Stack direction="row" spacing={2} sx={{ alignItems: "flex-start" }}>
          <ContentTypePicker value={contentTypeName} onChange={setContentTypeName} />
          <Select
            value={language}
            onChange={(e) => setChosenLanguage(e.target.value)}
            size="small"
            sx={{ minWidth: 140 }}
            renderValue={(l) => t("cmsCreatePage.masterLanguage", { language: l })}
          >
            {supportedLanguages.map((l) => (
              <MenuItem key={l} value={l}>
                {l}
              </MenuItem>
            ))}
          </Select>
        </Stack>
        {contentTypeName ? (
          <CreateForm key={contentTypeName + language} contentTypeName={contentTypeName} language={language} />
        ) : (
          <Box sx={{ display: "flex", flexDirection: "column", alignItems: "center", gap: 1, py: 5, color: "text.secondary" }}>
            <NoteAdd sx={{ fontSize: 32 }} />
            <Typography variant="body2">{t("cmsCreatePage.chooseTypeEmpty")}</Typography>
          </Box>
        )}
      </Stack>
    </Stack>
  );
}

function CreateForm({ contentTypeName, language }: { contentTypeName: string; language: string }) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { showToast } = useToast();
  const formRef = useRef<ContentFormHandle>(null);
  const { data: schema, isLoading } = useQuery({
    queryKey: ["creation-schema", contentTypeName, language],
    queryFn: () => api.getCreationSchema(contentTypeName, language),
  });
  const [draft, setDraft] = useState<CreateContentSchema | undefined>(undefined);
  const active = draft ?? schema;

  if (isLoading || !active)
    return (
      <Stack spacing={2} sx={{ mt: 2 }}>
        <Skeleton variant="rounded" height={56} />
        <Skeleton variant="rounded" height={56} />
        <Skeleton variant="rounded" height={100} />
      </Stack>
    );

  return (
    <Stack
      spacing={2}
      onKeyDown={(e) => {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "s") {
          e.preventDefault();
          formRef.current?.submit();
        }
      }}
    >
      <TextField
        label={t("cmsCreatePage.nameLabel")}
        fullWidth
        value={active.metadata.name}
        onChange={(e) => setDraft({ ...active, metadata: { ...active.metadata, name: e.target.value } })}
      />
      <ContentForm
        ref={formRef}
        contentTypeName={active.metadata.contentTypeKey}
        language={active.metadata.language}
        properties={active.properties}
        submitText={t("cmsCreatePage.createSubmit")}
        onChange={(key, value) =>
          setDraft({ ...active, properties: { ...active.properties, [key]: { ...active.properties[key], value } } })
        }
        onSubmit={async () => {
          try {
            const created = await api.createContent(active);
            showToast(t("cmsCreatePage.createdToast"));
            navigate(`/cms/edit/${created.metadata.id}`);
          } catch (error) {
            showToast(errorMessage(error, t("cmsCreatePage.createFailed")), "error");
          }
        }}
      />
    </Stack>
  );
}
