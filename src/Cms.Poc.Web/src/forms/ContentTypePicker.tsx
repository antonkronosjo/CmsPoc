import { useQuery } from "@tanstack/react-query";
import { MenuItem, Select } from "@mui/material";
import { api } from "../api/client";

interface ContentTypePickerProps {
  value: string;
  onChange: (value: string) => void;
}

export default function ContentTypePicker({ value, onChange }: ContentTypePickerProps) {
  const { data: contentTypes = [] } = useQuery({ queryKey: ["content-types"], queryFn: api.getContentTypes });

  return (
    <Select displayEmpty value={value} onChange={(e) => onChange(e.target.value)} size="small" sx={{ minWidth: 220, mb: 2 }}>
      <MenuItem value="">
        <em>Choose a content type</em>
      </MenuItem>
      {contentTypes.map((t) => (
        <MenuItem key={t} value={t}>
          {t}
        </MenuItem>
      ))}
    </Select>
  );
}
