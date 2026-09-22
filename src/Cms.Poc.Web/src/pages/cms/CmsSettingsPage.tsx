import { useEffect, useState } from "react";
import { Navigate } from "react-router-dom";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Alert, Button, Card, MenuItem, Select, Stack, TextField, Typography } from "@mui/material";
import { api, CmsRole } from "../../api/client";
import { useUser } from "../../context/UserContext";

export default function CmsSettingsPage() {
  const { isInRole } = useUser();
  const { data, isLoading } = useQuery({ queryKey: ["languages"], queryFn: api.getLanguages });
  const queryClient = useQueryClient();

  const [supportedLanguagesText, setSupportedLanguagesText] = useState("");
  const [defaultLanguage, setDefaultLanguage] = useState("");
  const [saving, setSaving] = useState(false);
  const [result, setResult] = useState<{ kind: "success" | "error"; message: string } | undefined>(undefined);

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
    setResult(undefined);
    try {
      await api.updateLanguages({ defaultLanguage, supportedLanguages });
      await queryClient.invalidateQueries({ queryKey: ["languages"] });
      setResult({ kind: "success", message: "Settings saved." });
    } catch (e) {
      setResult({ kind: "error", message: e instanceof Error ? e.message : "Failed to save settings." });
    } finally {
      setSaving(false);
    }
  }

  if (!isInRole(CmsRole.Admin)) return <Navigate to="/cms" replace />;
  if (isLoading) return <Typography color="text.secondary">Loading…</Typography>;

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Settings</Typography>
      <Card sx={{ p: 3 }}>
        <Stack spacing={2} sx={{ maxWidth: 420 }}>
          <Typography variant="subtitle1">Available languages</Typography>
          <TextField
            label="Supported languages"
            helperText="Comma-separated language codes, e.g. en, sv"
            value={supportedLanguagesText}
            onChange={(e) => setSupportedLanguagesText(e.target.value)}
            fullWidth
          />
          <Select
            value={supportedLanguages.includes(defaultLanguage) ? defaultLanguage : ""}
            onChange={(e) => setDefaultLanguage(e.target.value)}
            displayEmpty
            renderValue={(l) => (l ? `Default language: ${l}` : "Choose a default language")}
            disabled={supportedLanguages.length === 0}
          >
            {supportedLanguages.map((l) => (
              <MenuItem key={l} value={l}>
                {l}
              </MenuItem>
            ))}
          </Select>
          {result && <Alert severity={result.kind}>{result.message}</Alert>}
          <Button
            variant="contained"
            onClick={save}
            disabled={saving || supportedLanguages.length === 0 || !supportedLanguages.includes(defaultLanguage)}
            sx={{ alignSelf: "flex-start" }}
          >
            Save
          </Button>
        </Stack>
      </Card>
    </Stack>
  );
}
