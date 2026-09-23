import { MenuItem, Select } from "@mui/material";
import { useContentTypes } from "../hooks/useContentTypes";

interface ContentTypePickerProps {
  value: string;
  onChange: (value: string) => void;
}

export default function ContentTypePicker({ value, onChange }: ContentTypePickerProps) {
  const { data: contentTypes = [] } = useContentTypes();

  return (
    <Select displayEmpty value={value} onChange={(e) => onChange(e.target.value)} size="small" sx={{ minWidth: 220 }}>
      <MenuItem value="">
        <em>Choose a content type</em>
      </MenuItem>
      {contentTypes.map((t) => (
        <MenuItem key={t.key} value={t.key}>
          {t.key}
        </MenuItem>
      ))}
    </Select>
  );
}
