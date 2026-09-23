import { useEffect, useState } from "react";
import { Navigate, Link as RouterLink } from "react-router-dom";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Breadcrumbs, Button, CircularProgress, MenuItem, Select, Skeleton, Stack, TextField, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { api, CmsRole } from "../../api/client";
import { useUser } from "../../context/UserContext";
import { useToast, errorMessage } from "../../context/ToastContext";

export default function CmsSettingsPage() {
  const { t } = useTranslation();
  const { isInRole } = useUser();
  const { data, isLoading } = useQuery({ queryKey: ["languages"], queryFn: api.getLanguages });
  const queryClient = useQueryClient();
  const { showToast } = useToast();

  const [supportedLanguagesText, setSupportedLanguagesText] = useState("");
  const [defaultLanguage, setDefaultLanguage] = useState("");
  const [saving, setSaving] = useState(false);

  // Seed the draft from the loaded settings once, not on every refetch, so mid-edit isn't clobbered.
  useEffect(() => {
    if (data && supportedLanguagesText === "") {
      setSupportedLanguagesText(data.supportedLanguages.join(", "));
      setDefaultLanguage(data.defaultLanguage);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [data]);

  const supportedLanguages = Array.from(
    new Set(
      supportedLanguagesText
        .split(",")
        .map((l) => l.trim())
        .filter((l) => l.length > 0),
    ),
  );

  async function save() {
    setSaving(true);
    try {
      await api.updateLanguages({ defaultLanguage, supportedLanguages });
      await queryClient.invalidateQueries({ queryKey: ["languages"] });
      showToast(t("cmsSettingsPage.savedToast"));
    } catch (e) {
      showToast(errorMessage(e, t("cmsSettingsPage.saveFailed")), "error");
    } finally {
      setSaving(false);
    }
  }

  if (!isInRole(CmsRole.Admin)) return <Navigate to="/cms" replace />;

  if (isLoading)
    return (
      <Stack spacing={2}>
        <Skeleton variant="text" width={120} height={40} />
        <Skeleton variant="rounded" height={260} />
      </Stack>
    );

  return (
    <Stack spacing={2}>
      <Breadcrumbs>
        <Typography component={RouterLink} to="/cms" color="primary" sx={{ textDecoration: "none", fontWeight: 500, "&:hover": { textDecoration: "underline" } }}>
          {t("cmsSettingsPage.breadcrumbBrowse")}
        </Typography>
        <Typography color="text.secondary">{t("cmsSettingsPage.breadcrumbSettings")}</Typography>
      </Breadcrumbs>
      <Typography variant="h5">{t("cmsSettingsPage.heading")}</Typography>
      <Stack spacing={2} sx={{ maxWidth: 420 }}>
        <Typography variant="subtitle1">{t("cmsSettingsPage.availableLanguages")}</Typography>
        <TextField
          label={t("cmsSettingsPage.supportedLanguagesLabel")}
          helperText={t("cmsSettingsPage.supportedLanguagesHelper")}
          value={supportedLanguagesText}
          onChange={(e) => setSupportedLanguagesText(e.target.value)}
          fullWidth
        />
        <Select
          value={supportedLanguages.includes(defaultLanguage) ? defaultLanguage : ""}
          onChange={(e) => setDefaultLanguage(e.target.value)}
          displayEmpty
          renderValue={(l) => (l ? t("cmsSettingsPage.defaultLanguageSelected", { language: l }) : t("cmsSettingsPage.chooseDefaultLanguage"))}
          disabled={supportedLanguages.length === 0}
        >
          {supportedLanguages.map((l) => (
            <MenuItem key={l} value={l}>
              {l}
            </MenuItem>
          ))}
        </Select>
        <Button
          variant="contained"
          onClick={save}
          disabled={saving || supportedLanguages.length === 0 || !supportedLanguages.includes(defaultLanguage)}
          startIcon={saving ? <CircularProgress size={16} color="inherit" /> : undefined}
          sx={{ alignSelf: "flex-start" }}
        >
          {saving ? t("cmsSettingsPage.saving") : t("cmsSettingsPage.save")}
        </Button>
      </Stack>
    </Stack>
  );
}
