import { useEffect, useState } from "react";
import { api, type ContentSearchResult, type EventContent, type NewsContent } from "./api";
import "./App.css";

const LANGUAGES = ["en", "sv"];

type Selected = { type: "NewsContent"; id: number } | { type: "EventContent"; id: number } | null;

function App() {
  const [lang, setLang] = useState("en");
  const [searchName, setSearchName] = useState("HEJ");
  const [results, setResults] = useState<ContentSearchResult[]>([]);
  const [selected, setSelected] = useState<Selected>(null);
  const [error, setError] = useState<string | null>(null);

  const runSearch = async () => {
    setError(null);
    try {
      setResults(await api.searchContent(searchName, lang));
    } catch (e) {
      setError(String(e));
    }
  };

  return (
    <div className="app">
      <header>
        <h1>Content Framework POC</h1>
        <p className="subtitle">
          Flat <code>NewsContent</code> / <code>EventContent</code> models, generated persistence, immutable versioning.
        </p>
        <label className="lang-picker">
          Language:{" "}
          <select value={lang} onChange={(e) => setLang(e.target.value)}>
            {LANGUAGES.map((l) => (
              <option key={l} value={l}>{l}</option>
            ))}
          </select>
        </label>
      </header>

      <main>
        <section className="panel">
          <h2>Create content</h2>
          <CreatePanel lang={lang} onCreated={() => runSearch()} />
        </section>

        <section className="panel">
          <h2>
            Polymorphic search — <code>Query&lt;Content&gt;().Where(x =&gt; x.Name == name)</code>
          </h2>
          <div className="search-row">
            <input value={searchName} onChange={(e) => setSearchName(e.target.value)} placeholder="Name" />
            <button onClick={runSearch}>Search</button>
          </div>
          {error && <p className="error">{error}</p>}
          <ul className="result-list">
            {results.map((r) => (
              <li key={`${r.type}-${r.id}`}>
                <button className="result-item" onClick={() => setSelected({ type: r.type as "NewsContent" | "EventContent", id: r.id })}>
                  <span className={`badge badge-${r.type}`}>{r.type}</span>
                  <span>#{r.id} "{r.name}" — v{r.versionNumber} ({r.language})</span>
                </button>
              </li>
            ))}
          </ul>
        </section>

        <section className="panel">
          <h2>Detail &amp; version history</h2>
          {selected ? <DetailPanel selected={selected} lang={lang} onUpdated={runSearch} /> : <p>Select a search result above.</p>}
        </section>
      </main>
    </div>
  );
}

function CreatePanel({ lang, onCreated }: { lang: string; onCreated: () => void }) {
  const [kind, setKind] = useState<"NewsContent" | "EventContent">("NewsContent");
  const [name, setName] = useState("HEJ");
  const [heading, setHeading] = useState("Hello");
  const [body, setBody] = useState("World");
  const [color, setColor] = useState("red");
  const [title, setTitle] = useState("Conference");
  const [description, setDescription] = useState("Annual conference");
  const [startDate, setStartDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [status, setStatus] = useState<string | null>(null);

  const submit = async () => {
    setStatus(null);
    try {
      if (kind === "NewsContent") {
        const created = await api.createNews({ name, language: lang, heading, body, color });
        setStatus(`Created NewsContent #${created.id} (v${created.versionNumber})`);
      } else {
        const created = await api.createEvent({ name, language: lang, title, description, startDate: new Date(startDate).toISOString() });
        setStatus(`Created EventContent #${created.id} (v${created.versionNumber})`);
      }
      onCreated();
    } catch (e) {
      setStatus(String(e));
    }
  };

  return (
    <div className="create-panel">
      <div className="kind-toggle">
        <label>
          <input type="radio" checked={kind === "NewsContent"} onChange={() => setKind("NewsContent")} /> NewsContent
        </label>
        <label>
          <input type="radio" checked={kind === "EventContent"} onChange={() => setKind("EventContent")} /> EventContent
        </label>
      </div>

      <label>Name <input value={name} onChange={(e) => setName(e.target.value)} /></label>

      {kind === "NewsContent" ? (
        <>
          <label>Heading ({lang}) <input value={heading} onChange={(e) => setHeading(e.target.value)} /></label>
          <label>Body ({lang}) <input value={body} onChange={(e) => setBody(e.target.value)} /></label>
          <label>Color (invariant) <input value={color} onChange={(e) => setColor(e.target.value)} /></label>
        </>
      ) : (
        <>
          <label>Title ({lang}) <input value={title} onChange={(e) => setTitle(e.target.value)} /></label>
          <label>Description ({lang}) <input value={description} onChange={(e) => setDescription(e.target.value)} /></label>
          <label>Start date (invariant) <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} /></label>
        </>
      )}

      <button onClick={submit}>Create</button>
      {status && <p className="status">{status}</p>}
    </div>
  );
}

function DetailPanel({ selected, lang, onUpdated }: { selected: NonNullable<Selected>; lang: string; onUpdated: () => void }) {
  const [news, setNews] = useState<NewsContent | null>(null);
  const [event, setEvent] = useState<EventContent | null>(null);
  const [history, setHistory] = useState<(NewsContent | EventContent)[]>([]);
  const [status, setStatus] = useState<string | null>(null);

  const load = async () => {
    setStatus(null);
    if (selected.type === "NewsContent") {
      const [current, hist] = await Promise.all([api.getNews(selected.id, lang), api.historyNews(selected.id, lang)]);
      setNews(current);
      setEvent(null);
      setHistory(hist);
    } else {
      const [current, hist] = await Promise.all([api.getEvent(selected.id, lang), api.historyEvent(selected.id, lang)]);
      setEvent(current);
      setNews(null);
      setHistory(hist);
    }
  };

  // Reload whenever the selection or language changes.
  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selected.type, selected.id, lang]);

  const save = async () => {
    setStatus(null);
    try {
      if (selected.type === "NewsContent" && news) {
        const updated = await api.updateNews(news.id, { language: lang, heading: news.heading, body: news.body, color: news.color });
        setStatus(`Saved — now v${updated.versionNumber}`);
      } else if (event) {
        const updated = await api.updateEvent(event.id, { language: lang, title: event.title, description: event.description, startDate: event.startDate });
        setStatus(`Saved — now v${updated.versionNumber}`);
      }
      await load();
      onUpdated();
    } catch (e) {
      setStatus(String(e));
    }
  };

  return (
    <div className="detail-panel">
      {news && (
        <div className="edit-form">
          <span className="badge badge-NewsContent">NewsContent #{news.id}</span>
          <label>Heading ({lang}) <input value={news.heading} onChange={(e) => setNews({ ...news, heading: e.target.value })} /></label>
          <label>Body ({lang}) <input value={news.body} onChange={(e) => setNews({ ...news, body: e.target.value })} /></label>
          <label>Color (invariant) <input value={news.color} onChange={(e) => setNews({ ...news, color: e.target.value })} /></label>
          <button onClick={save}>Save (creates a new version)</button>
        </div>
      )}
      {event && (
        <div className="edit-form">
          <span className="badge badge-EventContent">EventContent #{event.id}</span>
          <label>Title ({lang}) <input value={event.title} onChange={(e) => setEvent({ ...event, title: e.target.value })} /></label>
          <label>Description ({lang}) <input value={event.description} onChange={(e) => setEvent({ ...event, description: e.target.value })} /></label>
          <button onClick={save}>Save (creates a new version)</button>
        </div>
      )}
      {status && <p className="status">{status}</p>}

      <h3>Version history ({lang})</h3>
      <table className="history-table">
        <thead>
          <tr>
            <th>Version</th>
            <th>Created (UTC)</th>
            <th>Fields</th>
          </tr>
        </thead>
        <tbody>
          {history.map((h) => (
            <tr key={h.versionNumber}>
              <td>v{h.versionNumber}</td>
              <td>{new Date(h.createdAtUtc).toLocaleString()}</td>
              <td>
                {"heading" in h
                  ? `${h.heading} / ${h.body} / ${h.color}`
                  : `${h.title} / ${h.description} / ${new Date(h.startDate).toLocaleDateString()}`}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default App;
