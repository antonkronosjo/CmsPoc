const API_BASE = "http://localhost:5075";

export const InputType = {
  Text: 0,
  TextArea: 1,
  Number: 2,
  Date: 3,
  DateTime: 4,
  ContentReference: 5,
} as const;
export type InputType = (typeof InputType)[keyof typeof InputType];

/// Both the metadata needed to render a field and the field's current
/// value - the same shape flows from schema to edited state to submit body.
export interface ContentPropertyValueDto {
  inputType: InputType;
  required: boolean;
  value: unknown;
}

export interface CreateContentMetadata {
  contentTypeName: string;
  language: string;
  name: string;
}

export interface CreateContentSchema {
  metadata: CreateContentMetadata;
  properties: Record<string, ContentPropertyValueDto>;
}

export interface UpdateContentMetadata {
  id: number;
  contentTypeName: string;
  language: string;
  name: string;
  versionNumber: number;
  createdAtUtc: string;
}

/// Returned by the update-schema GET, required as-is for the update PUT
/// body, and returned again by that same PUT - one type for the whole
/// round trip.
export interface UpdateContentSchema {
  metadata: UpdateContentMetadata;
  properties: Record<string, ContentPropertyValueDto>;
}

export interface ContentSummaryDto {
  id: number;
  contentTypeName: string;
  name: string;
  language: string;
  versionNumber: number;
  createdAtUtc: string;
  properties: Record<string, unknown>;
}

async function json<T>(res: Response): Promise<T> {
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
  return (await res.json()) as T;
}

function query(params: Record<string, string | number | undefined>): string {
  const usp = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined) usp.set(key, String(value));
  }
  const qs = usp.toString();
  return qs ? `?${qs}` : "";
}

export const api = {
  getContentTypes: () => fetch(`${API_BASE}/api/content/types`).then((r) => json<string[]>(r)),

  getCreationSchema: (contentTypeName: string, language: string) =>
    fetch(`${API_BASE}/api/content/creationschema${query({ contentTypeName, language })}`).then((r) => json<CreateContentSchema>(r)),

  createContent: (request: CreateContentSchema) =>
    fetch(`${API_BASE}/api/content`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
    }).then((r) => json<UpdateContentSchema>(r)),

  getUpdateSchema: (id: number, language: string) =>
    fetch(`${API_BASE}/api/content/${id}/updateschema${query({ language })}`).then((r) => json<UpdateContentSchema>(r)),

  updateContent: (id: number, request: UpdateContentSchema) =>
    fetch(`${API_BASE}/api/content/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
    }).then((r) => json<UpdateContentSchema>(r)),

  getContentSummary: (id: number, language: string) =>
    fetch(`${API_BASE}/api/content/${id}${query({ language })}`).then((r) => json<ContentSummaryDto>(r)),

  searchContent: (searchQuery: string, language: string) =>
    fetch(`${API_BASE}/api/content/search${query({ query: searchQuery, language })}`).then((r) => json<ContentSummaryDto[]>(r)),

  getHistory: (id: number, language: string) =>
    fetch(`${API_BASE}/api/content/${id}/history${query({ language })}`).then((r) => json<ContentSummaryDto[]>(r)),

  validateProperty: (contentTypeName: string, propertyName: string, value: ContentPropertyValueDto) =>
    fetch(`${API_BASE}/api/content/validate${query({ contentTypeName, propertyName })}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(value),
    }).then((r) => json<string[]>(r)),
};
