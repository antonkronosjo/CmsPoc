const API_BASE = "http://localhost:5075";

export const InputType = {
  Text: "Text",
  TextArea: "TextArea",
  Number: "Number",
  Date: "Date",
  DateTime: "DateTime",
  ContentReference: "ContentReference",
} as const;
export type InputType = (typeof InputType)[keyof typeof InputType];

/// Mirrors the backend ContentReference struct: which content item, and its content type key.
export interface ContentReference {
  id: number;
  contentType: string;
}

/// Both the metadata needed to render a field and the field's current
/// value - the same shape flows from schema to edited state to submit body.
export interface ContentPropertyValueDto {
  inputType: InputType;
  required: boolean;
  /// True when the value differs per language; everything else is shared and only editable in the master language.
  cultureSpecific: boolean;
  value: unknown;
}

/// A registered content type.
export interface ContentTypeInfo {
  key: string;
}

export interface LanguageSettingsDto {
  defaultLanguage: string;
  supportedLanguages: string[];
}

export interface CreateContentMetadata {
  contentTypeKey: string;
  language: string;
  name: string;
}

export interface CreateContentSchema {
  metadata: CreateContentMetadata;
  properties: Record<string, ContentPropertyValueDto>;
}

export interface UpdateContentMetadata {
  id: number;
  contentTypeKey: string;
  language: string;
  /// The language the item was created in - the only one where shared properties and the name are editable.
  masterLanguage: string;
  name: string;
  versionNumber: number;
  created: string;
  startPublish: string | null;
  stopPublish: string | null;
  /// The version number currently live for this content item, or null if none is - may differ from versionNumber.
  livePublishedVersionNumber: number | null;
  /// The most recent version number for this branch, or null if the branch has no history yet - may differ from versionNumber.
  latestVersionNumber: number | null;
  /// Languages that have a version branch.
  languages: string[];
}

/// Returned by the update-schema GET, required as-is for the update PUT
/// body, and returned again by that same PUT - one type for the whole
/// round trip.
export interface UpdateContentSchema {
  metadata: UpdateContentMetadata;
  properties: Record<string, ContentPropertyValueDto>;
}

/// A reference to the user behind an action. displayName comes from the host's user adapter;
/// removed is true when that adapter no longer knows the id (e.g. an erased user).
export interface UserRefDto {
  id: string;
  displayName: string | null;
  removed: boolean;
}

/// One language branch's publish state: enough to classify it as Published, Scheduled, Unpublished or Draft via getPublishStatus/getBranchPublishStatus.
export interface LanguageStatusDto {
  language: string;
  versionNumber: number;
  startPublish: string | null;
  livePublishedVersionNumber: number | null;
  /// Whether ANY version of this branch, ever, has had a publish window set - not just the latest one's.
  hasBeenPublished: boolean;
}

export interface ContentSummaryDto {
  id: number;
  contentTypeKey: string;
  name: string;
  language: string;
  masterLanguage: string;
  versionNumber: number;
  created: string;
  startPublish: string | null;
  stopPublish: string | null;
  /// The version number currently live for this content item, or null if none is - may differ from versionNumber.
  livePublishedVersionNumber: number | null;
  /// The most recent version number for this branch, or null if the branch has no history yet - may differ from versionNumber.
  latestVersionNumber: number | null;
  /// Languages that have a version branch (empty for version-history rows).
  languages: string[];
  /// Each language in `languages`'s own publish state, resolved live (empty for version-history rows).
  languageStatuses: LanguageStatusDto[];
  /// When the item itself was first created - invariant across every language and version, unlike `created` (default for version-history rows).
  rootCreated: string;
  /// The most recent change to the item as a whole: the newest `created` across every version, in every language branch, not just `language`'s (default for version-history rows).
  lastModified: string;
  /// Null when user tracking is off or the reference was removed.
  createdBy: UserRefDto | null;
  publishedBy: UserRefDto | null;
  properties: Record<string, unknown>;
}

export interface SearchContentResult {
  items: ContentSummaryDto[];
  totalCount: number;
}

export interface SearchContentOptions {
  contentTypeKey?: string;
  /// Filters to any of several content types at once (e.g. a news type plus its variants).
  /// Takes priority over contentTypeKey when both are given.
  contentTypeKeys?: string[];
  page?: number;
  pageSize?: number;
  publishedOnly?: boolean;
  /// A ContentSummaryDto field name (case-insensitive), e.g. "name" or "lastModified". Unknown
  /// names or non-comparable fields (like "languages") are ignored server-side, not an error.
  sortBy?: string;
  sortDescending?: boolean;
}

export interface PublishContentRequest {
  language: string;
  versionNumber: number;
  startPublish?: string | null;
  stopPublish?: string | null;
}

async function json<T>(res: Response): Promise<T> {
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
  return (await res.json()) as T;
}

async function ensureOk(res: Response): Promise<void> {
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
}

function query(params: Record<string, string | number | boolean | string[] | undefined>): string {
  const usp = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined) continue;
    if (Array.isArray(value)) {
      for (const v of value) usp.append(key, v);
    } else {
      usp.set(key, String(value));
    }
  }
  const qs = usp.toString();
  return qs ? `?${qs}` : "";
}

export const CmsRole = {
  Editor: "Editor",
  Admin: "Admin",
} as const;
export type CmsRole = (typeof CmsRole)[keyof typeof CmsRole];

/// The caller's status as reported by the backend: whether they are signed in, their display name, and which CMS roles they hold.
export interface CurrentUserDto {
  isAuthenticated: boolean;
  displayName: string | null;
  roles: CmsRole[];
}

export const api = {
  getCurrentUser: () => fetch(`${API_BASE}/api/user`).then((r) => json<CurrentUserDto>(r)),

  getLanguages: () => fetch(`${API_BASE}/api/settings/languages`).then((r) => json<LanguageSettingsDto>(r)),

  updateLanguages: (request: LanguageSettingsDto) =>
    fetch(`${API_BASE}/api/settings/languages`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
    }).then(ensureOk),

  getContentTypes: () => fetch(`${API_BASE}/api/content/types`).then((r) => json<ContentTypeInfo[]>(r)),

  getCreationSchema: (contentTypeKey: string, language: string) =>
    fetch(`${API_BASE}/api/content/creationschema${query({ contentTypeKey, language })}`).then((r) => json<CreateContentSchema>(r)),

  createContent: (request: CreateContentSchema) =>
    fetch(`${API_BASE}/api/content`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
    }).then((r) => json<UpdateContentSchema>(r)),

  getUpdateSchema: (id: number, language: string, version?: number) =>
    fetch(`${API_BASE}/api/content/${id}/updateschema${query({ language, version })}`).then((r) => json<UpdateContentSchema>(r)),

  updateContent: (id: number, request: UpdateContentSchema) =>
    fetch(`${API_BASE}/api/content/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
    }).then((r) => json<UpdateContentSchema>(r)),

  getContentSummary: (id: number, language?: string) =>
    fetch(`${API_BASE}/api/content/${id}${query({ language })}`).then((r) => json<ContentSummaryDto>(r)),

  searchContent: (searchQuery: string, language: string | undefined, options: SearchContentOptions = {}) =>
    fetch(
      `${API_BASE}/api/content/search${query({
        query: searchQuery,
        language,
        contentTypeKey: options.contentTypeKey,
        contentTypeKeys: options.contentTypeKeys,
        page: options.page,
        pageSize: options.pageSize,
        publishedOnly: options.publishedOnly,
        sortBy: options.sortBy,
        sortDescending: options.sortDescending,
      })}`,
    ).then((r) => json<SearchContentResult>(r)),

  getHistory: (id: number, language: string) =>
    fetch(`${API_BASE}/api/content/${id}/history${query({ language })}`).then((r) => json<ContentSummaryDto[]>(r)),

  validateProperty: (contentTypeKey: string, propertyName: string, value: ContentPropertyValueDto) =>
    fetch(`${API_BASE}/api/content/validate${query({ contentTypeKey, propertyName })}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(value),
    }).then((r) => json<string[]>(r)),

  publishContent: (id: number, request: PublishContentRequest) =>
    fetch(`${API_BASE}/api/content/${id}/publish`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
    }).then(ensureOk),

  unpublishContent: (id: number, language: string) =>
    fetch(`${API_BASE}/api/content/${id}/unpublish${query({ language })}`, { method: "POST" }).then(ensureOk),
};
