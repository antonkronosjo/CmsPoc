import { useQuery } from "@tanstack/react-query";
import { api } from "../api/client";

/// The languages the backend offers for translating content, and the one
/// pre-selected for new content. Shared query key so every consumer hits one cached request.
export function useLanguages() {
  const { data } = useQuery({ queryKey: ["languages"], queryFn: api.getLanguages, staleTime: Infinity });
  return {
    loaded: data !== undefined,
    defaultLanguage: data?.defaultLanguage ?? "en",
    supportedLanguages: data?.supportedLanguages ?? ["en"],
  };
}
