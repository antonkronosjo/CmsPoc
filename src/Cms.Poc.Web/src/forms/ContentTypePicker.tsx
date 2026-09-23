import { MenuItem, Select } from "@mui/material";
import { useTranslation } from "react-i18next";
import { useContentTypes } from "../hooks/useContentTypes";

interface ContentTypePickerProps {
  value: string;
  onChange: (value: string) => void;
}

export default function ContentTypePicker({ value, onChange }: ContentTypePickerProps) {
  const { t } = useTranslation();
  const { data: contentTypes = [] } = useContentTypes();

  return (
    <Select displayEmpty value={value} onChange={(e) => onChange(e.target.value)} size="small" sx={{ minWidth: 220 }}>
      <MenuItem value="">
        <em>{t("contentTypePicker.choosePlaceholder")}</em>
      </MenuItem>
      {contentTypes.map((contentType) => (
        <MenuItem key={contentType.key} value={contentType.key}>
          {contentType.key}
        </MenuItem>
      ))}
    </Select>
  );
}
