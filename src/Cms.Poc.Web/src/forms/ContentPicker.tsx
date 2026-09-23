import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Dialog, DialogContent, DialogTitle, IconButton, InputAdornment, TextField, Typography } from "@mui/material";
import { Close } from "@mui/icons-material";
import { api, type ContentReference } from "../api/client";
import ContentSearchList from "../components/ContentSearchList";

interface ContentPickerProps {
  label: string;
  helperText?: string;
  value: ContentReference | null | undefined;
  onChange: (value: ContentReference | null) => void;
  disabled?: boolean;
}

/// The extensibility showcase: a property template more complex than a
/// plain input, resolving another content item's display name and letting
/// the user replace it through a searchable dialog. Registering it took one
/// InputType enum value and one case in ContentForm's template switch.
export default function ContentPicker({ label, helperText, value, onChange, disabled }: ContentPickerProps) {
  const [open, setOpen] = useState(false);

  const { data: resolved } = useQuery({
    queryKey: ["content-summary", value?.id, "all"],
    queryFn: () => api.getContentSummary(value!.id),
    enabled: value != null,
  });

  return (
    <>
      <TextField
        label={label}
        fullWidth
        value=""
        helperText={helperText}
        disabled={disabled}
        onClick={() => !disabled && setOpen(true)}
        slotProps={{
          input: {
            readOnly: true,
            sx: { cursor: disabled ? "default" : "pointer", caretColor: "transparent" },
            startAdornment:
              value != null ? (
                <InputAdornment position="start">
                  <Typography variant="body2" color="text.secondary">
                    {resolved?.contentTypeKey ?? value.contentType}
                  </Typography>
                  <Typography variant="body1" color="text.primary" sx={{ ml: 1 }}>
                    {resolved ? resolved.name || "(untitled)" : `#${value.id}`}
                  </Typography>
                  {!disabled && (
                    <IconButton
                      size="small"
                      aria-label="clear selection"
                      onClick={(e) => {
                        e.stopPropagation();
                        onChange(null);
                      }}
                      onMouseDown={(e) => e.stopPropagation()}
                      sx={{ ml: 0.5 }}
                    >
                      <Close fontSize="small" />
                    </IconButton>
                  )}
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
            excludeId={value?.id}
            onSelect={(item) => {
              onChange({ id: item.id, contentType: item.contentTypeKey });
              setOpen(false);
            }}
          />
        </DialogContent>
      </Dialog>
    </>
  );
}
