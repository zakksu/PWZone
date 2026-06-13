import type { ResourceFilter, ServerStatusMessage } from '../types';
import { IS_GITHUB_PAGES } from '../config';

interface SidebarProps {
  filter: ResourceFilter;
  onFilterChange: (filter: ResourceFilter) => void;
  searchQuery: string;
  onSearchChange: (q: string) => void;
  connected: boolean;
  status: ServerStatusMessage | null;
  lastError: string | null;
  nearestName: string | null;
  nearestDistance: number;
  resourceCount: number;
  onZoomNearest: () => void;
  offlineDemo: boolean;
  onOfflineDemoChange: (enabled: boolean) => void;
  isDemoActive: boolean;
}

const FILTER_LABELS: Record<keyof ResourceFilter, string> = {
  herb: 'Herbs',
  ore: 'Ores',
  wood: 'Wood',
  plant: 'Plants',
};

export function Sidebar({
  filter,
  onFilterChange,
  searchQuery,
  onSearchChange,
  connected,
  status,
  lastError,
  nearestName,
  nearestDistance,
  resourceCount,
  onZoomNearest,
  offlineDemo,
  onOfflineDemoChange,
  isDemoActive,
}: SidebarProps) {
  return (
    <aside className="sidebar">
      <header className="sidebar-header">
        <h1>PW Companion</h1>
        <p className="subtitle">Filipe&apos;s Sarcastic PW Butler</p>
      </header>

      {IS_GITHUB_PAGES && (
        <section className="hosted-banner">
          <div className="field-label">Hosted map</div>
          <p className="muted" style={{ margin: 0, fontSize: '0.8rem' }}>
            Offline demo works here. For live PW tracking, run the desktop companion on your PC
            (it connects via localhost WebSocket).
          </p>
        </section>
      )}

      <section className="status-panel">
        <div className={`status-dot ${connected ? 'online' : 'offline'}`} />
        <div>
          <div className="status-label">
            {isDemoActive ? 'Offline demo' : connected ? 'Connected' : 'Disconnected'}
          </div>
          {isDemoActive && (
            <div className="status-detail">Fake player touring nodes — no game needed.</div>
          )}
          {status && !isDemoActive && (
            <div className="status-detail">
              {status.attached
                ? `Attached · ${status.processName}`
                : status.message}
            </div>
          )}
          {lastError && !isDemoActive && <div className="status-error">{lastError}</div>}
        </div>
      </section>

      {!connected && (
        <section className="demo-panel">
          <div className="field-label">No companion yet?</div>
          <button
            type="button"
            className={`btn-accent ${offlineDemo ? 'btn-active' : ''}`}
            onClick={() => onOfflineDemoChange(!offlineDemo)}
          >
            {offlineDemo ? 'Stop offline demo' : 'Start offline demo'}
          </button>
          <p className="muted" style={{ marginTop: 8 }}>
            Walks a fake player around sample nodes. Zero PW required.
          </p>
        </section>
      )}

      <section>
        <label className="field-label" htmlFor="search">Search resources</label>
        <input
          id="search"
          type="search"
          placeholder="Iron Ore, Ghost Flower..."
          value={searchQuery}
          onChange={(e) => onSearchChange(e.target.value)}
          className="search-input"
        />
      </section>

      <section>
        <div className="field-label">Filters</div>
        <div className="filter-grid">
          {(Object.keys(FILTER_LABELS) as Array<keyof ResourceFilter>).map((key) => (
            <label key={key} className="filter-chip">
              <input
                type="checkbox"
                checked={filter[key]}
                onChange={(e) =>
                  onFilterChange({ ...filter, [key]: e.target.checked })
                }
              />
              {FILTER_LABELS[key]}
            </label>
          ))}
        </div>
      </section>

      <section className="nearest-panel">
        <div className="field-label">Nearest resource</div>
        {nearestName ? (
          <>
            <div className="nearest-name">{nearestName}</div>
            <div className="nearest-distance">{nearestDistance.toFixed(1)} units away</div>
            <button type="button" className="btn-accent" onClick={onZoomNearest}>
              Zoom to nearest
            </button>
          </>
        ) : (
          <p className="muted">No matching resources on this map. Tragic.</p>
        )}
      </section>

      <footer className="sidebar-footer">
        <span>{resourceCount} nodes visible</span>
        <span className="muted">Read-only · pw187.theclassic.games</span>
      </footer>

      <style>{`
        .sidebar {
          width: 280px;
          min-width: 280px;
          background: var(--bg-panel);
          border-right: 1px solid var(--border);
          padding: 16px;
          display: flex;
          flex-direction: column;
          gap: 16px;
          overflow-y: auto;
        }
        .sidebar-header h1 {
          margin: 0;
          font-size: 1.25rem;
          color: var(--accent);
        }
        .subtitle {
          margin: 4px 0 0;
          font-size: 0.75rem;
          color: var(--text-muted);
        }
        .status-panel {
          display: flex;
          gap: 10px;
          align-items: flex-start;
          padding: 10px;
          background: rgba(0,0,0,0.2);
          border-radius: 8px;
        }
        .status-dot {
          width: 10px;
          height: 10px;
          border-radius: 50%;
          margin-top: 4px;
          flex-shrink: 0;
        }
        .status-dot.online { background: var(--success); box-shadow: 0 0 6px var(--success); }
        .status-dot.offline { background: var(--accent); }
        .status-label { font-weight: 600; font-size: 0.9rem; }
        .status-detail, .status-error { font-size: 0.75rem; color: var(--text-muted); margin-top: 2px; }
        .status-error { color: var(--accent); }
        .field-label {
          display: block;
          font-size: 0.75rem;
          text-transform: uppercase;
          letter-spacing: 0.05em;
          color: var(--text-muted);
          margin-bottom: 6px;
        }
        .search-input {
          width: 100%;
          padding: 8px 10px;
          border: 1px solid var(--border);
          border-radius: 6px;
          background: var(--bg-primary);
          color: var(--text);
        }
        .filter-grid { display: flex; flex-wrap: wrap; gap: 6px; }
        .filter-chip {
          display: flex;
          align-items: center;
          gap: 4px;
          font-size: 0.85rem;
          padding: 4px 8px;
          background: rgba(0,0,0,0.2);
          border-radius: 4px;
          cursor: pointer;
        }
        .nearest-panel {
          padding: 12px;
          background: rgba(233,69,96,0.08);
          border: 1px solid rgba(233,69,96,0.2);
          border-radius: 8px;
        }
        .nearest-name { font-weight: 600; }
        .nearest-distance { font-size: 0.85rem; color: var(--text-muted); margin: 4px 0 10px; }
        .btn-accent {
          width: 100%;
          padding: 8px;
          background: var(--accent);
          color: #fff;
          border: none;
          border-radius: 6px;
          cursor: pointer;
          font-weight: 600;
        }
        .btn-accent:hover { background: var(--accent-dim); }
        .btn-active { outline: 2px solid var(--success); }
        .demo-panel {
          padding: 12px;
          background: rgba(78, 204, 163, 0.08);
          border: 1px solid rgba(78, 204, 163, 0.25);
          border-radius: 8px;
        }
        .hosted-banner {
          padding: 10px;
          background: rgba(233, 69, 96, 0.08);
          border: 1px solid rgba(233, 69, 96, 0.2);
          border-radius: 8px;
        }
        .muted { color: var(--text-muted); font-size: 0.85rem; }
        .sidebar-footer {
          margin-top: auto;
          font-size: 0.7rem;
          color: var(--text-muted);
          display: flex;
          flex-direction: column;
          gap: 4px;
        }
      `}</style>
    </aside>
  );
}

export default Sidebar;
