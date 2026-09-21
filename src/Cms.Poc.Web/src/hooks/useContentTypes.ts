import { useQuery } from "@tanstack/react-query";
import { api } from "../api/client";

/// The registered content types (key + color). Shared query key so every
/// component reading types hits one cached request.
export function useContentTypes() {
  return useQuery({ queryKey: ["content-types"], queryFn: api.getContentTypes, staleTime: Infinity });
}

/// The hex color configured for a content type via the backend's
/// [ContentType(Color = ...)], or undefined while loading / when none is set.
export function useContentTypeColor(contentTypeKey: string): string | undefined {
  const { data } = useContentTypes();
  return data?.find((t) => t.key === contentTypeKey)?.color ?? undefined;
}
