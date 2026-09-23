import { Chip, type ChipProps } from "@mui/material";

interface ContentTypeChipProps extends ChipProps {
  contentTypeKey: string;
}

/// A neutral chip identifying a content type.
export default function ContentTypeChip({ contentTypeKey, label, ...rest }: ContentTypeChipProps) {
  return <Chip size="small" label={label ?? contentTypeKey} {...rest} />;
}
