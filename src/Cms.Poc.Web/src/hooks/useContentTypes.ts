import { useQuery } from "@tanstack/react-query";
import { api } from "../api/client";

/// The registered content types. Shared query key so every component
/// reading types hits one cached request.
export function useContentTypes() {
  return useQuery({ queryKey: ["content-types"], queryFn: api.getContentTypes, staleTime: Infinity });
}
