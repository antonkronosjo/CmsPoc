const API_BASE = "http://localhost:5075";

export interface NewsContent {
  id: number;
  name: string;
  language: string;
  versionNumber: number;
  createdAtUtc: string;
  heading: string;
  body: string;
  color: string;
}

export interface EventContent {
  id: number;
  name: string;
  language: string;
  versionNumber: number;
  createdAtUtc: string;
  title: string;
  description: string;
  startDate: string;
}

export interface ContentSearchResult {
  type: "NewsContent" | "EventContent" | string;
  id: number;
  name: string;
  language: string;
  versionNumber: number;
  fields: Record<string, unknown>;
}

async function json<T>(res: Response): Promise<T> {
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
  return (await res.json()) as T;
}

export const api = {
  listNews: (lang: string) => fetch(`${API_BASE}/api/news?lang=${lang}`).then((r) => json<NewsContent[]>(r)),
  getNews: (id: number, lang: string) => fetch(`${API_BASE}/api/news/${id}?lang=${lang}`).then((r) => json<NewsContent>(r)),
  historyNews: (id: number, lang: string) => fetch(`${API_BASE}/api/news/${id}/history?lang=${lang}`).then((r) => json<NewsContent[]>(r)),
  createNews: (body: { name: string; language: string; heading: string; body: string; color: string }) =>
    fetch(`${API_BASE}/api/news`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }).then((r) => json<NewsContent>(r)),
  updateNews: (id: number, body: { language: string; heading: string; body: string; color: string }) =>
    fetch(`${API_BASE}/api/news/${id}`, { method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }).then((r) => json<NewsContent>(r)),

  listEvents: (lang: string) => fetch(`${API_BASE}/api/events?lang=${lang}`).then((r) => json<EventContent[]>(r)),
  getEvent: (id: number, lang: string) => fetch(`${API_BASE}/api/events/${id}?lang=${lang}`).then((r) => json<EventContent>(r)),
  historyEvent: (id: number, lang: string) => fetch(`${API_BASE}/api/events/${id}/history?lang=${lang}`).then((r) => json<EventContent[]>(r)),
  createEvent: (body: { name: string; language: string; title: string; description: string; startDate: string }) =>
    fetch(`${API_BASE}/api/events`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }).then((r) => json<EventContent>(r)),
  updateEvent: (id: number, body: { language: string; title: string; description: string; startDate: string }) =>
    fetch(`${API_BASE}/api/events/${id}`, { method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }).then((r) => json<EventContent>(r)),

  searchContent: (name: string, lang: string) =>
    fetch(`${API_BASE}/api/content?name=${encodeURIComponent(name)}&lang=${lang}`).then((r) => json<ContentSearchResult[]>(r)),
};
