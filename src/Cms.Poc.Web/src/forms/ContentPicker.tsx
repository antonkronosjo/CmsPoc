import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Chip, Dialog, DialogContent, DialogTitle, IconButton, InputAdornment, TextField, Typography } from "@mui/material";
import { Close } from "@mui/icons-material";
import { api, type ContentReference } from "../api/client";
import ContentSearchList from "../components/ContentSearchList";

interface ContentPickerProps {
  label: string;
  language: string;
  value: ContentReference | null | undefined;
  onChange: (value: ContentReference | null) => void;
  disabled?: boolean;
}

/// The extensibility showcase: a property template more complex than a
/// plain input, resolving another content item's display name and letting
/// the user replace it through a searchable dialog. Registering it took one
/// InputType enum value and one case in ContentForm's template switch.
export default function ContentPicker({ label, language, value, onChange, disabled }: ContentPickerProps) {
  const [open, setOpen] = useState(false);

  const { data: resolved } = useQuery({
    queryKey: ["content-summary", value?.id, language],
    queryFn: () => api.getContentSummary(value!.id, language),
    enabled: value != null,
  });

  return (
    <>
      <TextField
        label={label}
        fullWidth
        value=""
        disabled={disabled}
        onClick={() => !disabled && setOpen(true)}
        slotProps={{
          input: {
            readOnly: true,
            sx: { cursor: disabled ? "default" : "pointer", caretColor: "transparent" },
            startAdornment:
              value != null ? (
                <InputAdornment position="start">
                  <Chip
                    color="primary"
                    size="small"
                    label={resolved ? `${resolved.name || "(untitled)"} · ${resolved.contentTypeName}` : `#${value.id}`}
                    onDelete={
                      disabled
                        ? undefined
                        : (e) => {
                            e.stopPropagation();
                            onChange(null);
                          }
                    }
                    onMouseDown={(e) => e.stopPropagation()}
                  />
                </InputAdornment>
              ) : undefined,
          },
        }}
      />

      <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          <Typography variant="h6">Select content</Typography>
          <IconButton aria-label="close" onClick={() => setOpen(false)} sx={{ position: "absolute", right: 8, top: 8 }}>
            <Close />
          </IconButton>
        </DialogTitle>
        <DialogContent>
          <ContentSearchList
            language={language}
            excludeId={value?.id}
            onSelect={(item) => {
              onChange({ id: item.id, contentType: item.contentTypeName });
              setOpen(false);
            }}
          />
        </DialogContent>
      </Dialog>
    </>
  );
}
