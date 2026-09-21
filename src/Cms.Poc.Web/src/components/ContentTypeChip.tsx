import { Chip, type ChipProps } from "@mui/material";
import { useContentTypeColor } from "../hooks/useContentTypes";

/// Black or white, whichever reads better on the given #RGB / #RRGGBB background.
function contrastText(hex: string): string {
  const h = hex.length === 4 ? hex.slice(1).replace(/./g, "$&$&") : hex.slice(1);
  const [r, g, b] = [0, 2, 4].map((i) => parseInt(h.slice(i, i + 2), 16));
  return (r * 299 + g * 587 + b * 114) / 1000 > 150 ? "#000000" : "#FFFFFF";
}

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
      sx={[color ? { backgroundColor: color, color: contrastText(color) } : {}, ...(Array.isArray(sx) ? sx : [sx])]}
      {...rest}
    />
  );
}
