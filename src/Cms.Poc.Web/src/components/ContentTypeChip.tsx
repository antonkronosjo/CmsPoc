import { Chip, type ChipProps } from "@mui/material";
import { useContentTypeColor } from "../hooks/useContentTypes";
import { chipColorSx } from "../lib/chipColor";

interface ContentTypeChipProps extends Omit<ChipProps, "color"> {
  contentTypeKey: string;
}

/// A chip identifying a content type, filled with the type's configured color.
/// Falls back to MUI's neutral chip when the type has no color.
export default function ContentTypeChip({ contentTypeKey, label, sx, ...rest }: ContentTypeChipProps) {
  const color = useContentTypeColor(contentTypeKey);
  return (
    <Chip
      size="small"
      label={label ?? contentTypeKey}
      sx={[color ? chipColorSx(color) : {}, ...(Array.isArray(sx) ? sx : [sx])]}
      {...rest}
    />
  );
}
