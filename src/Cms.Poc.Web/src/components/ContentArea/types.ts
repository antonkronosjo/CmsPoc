import type { ComponentType } from "react";
import type { ContentSummaryDto } from "../../api/client";

/// Props every content type template receives.
export interface ContentTemplateProps {
  content: ContentSummaryDto;
}

export type ContentTemplate = ComponentType<ContentTemplateProps>;

/// Reads a property off a content summary. The API keys properties by their
/// C# names (PascalCase); matching case-insensitively keeps templates robust
/// if the serializer's naming policy changes.
export function getProperty<T = string>(content: ContentSummaryDto, name: string): T | undefined {
  const key = Object.keys(content.properties).find((k) => k.toLowerCase() === name.toLowerCase());
  return key === undefined ? undefined : (content.properties[key] as T);
}
