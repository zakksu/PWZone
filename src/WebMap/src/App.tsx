import { useCallback, useMemo, useState, lazy, Suspense } from 'react';
import { usePlayerWebSocket } from './hooks/usePlayerWebSocket';
import { useOfflineDemo } from './hooks/useOfflineDemo';
import { Sidebar } from './components/Sidebar';
import { ErrorBoundary } from './components/ErrorBoundary';
import type { GatheringMapData, MapMeta, PlayerUpdateMessage, ResourceFilter } from './types';
import { DEFAULT_FILTER } from './types';
import { assetUrl } from './config';
import mapsCatalog from './data/maps.json';
import map1Resources from './data/resources/map_1.json';

const MapView = lazy(() =>
  import('./components/MapView').then((m) => ({ default: m.MapView })),
);

const DEFAULT_MAP: MapMeta = {
  mapId: 1,
  name: 'Archosaur Outskirts (Sample)',
  originX: 0,
  originZ: 0,
  scale: 2,
  imageWidth: 1024,
  imageHeight: 1024,
  tileUrl: assetUrl('maps/sample_map_1.svg'),
};

function findMapMeta(mapId: number): MapMeta {
  const found = (mapsCatalog.maps as MapMeta[]).find((m) => m.mapId === mapId);
  if (!found) return DEFAULT_MAP;
  return {
    ...found,
    tileUrl: found.tileUrl.startsWith('http') ? found.tileUrl : assetUrl(found.tileUrl),
  };
}

function loadResources(mapId: number): GatheringMapData {
  if (mapId === 1) return map1Resources as GatheringMapData;
  return { mapId, mapName: 'Unknown', nodes: [] };
}

export default function App() {
  const { player: livePlayer, status, connected, lastError } = usePlayerWebSocket();
  const [offlineDemo, setOfflineDemo] = useState(true);
  const demoPlayer = useOfflineDemo(offlineDemo && !connected);
  const player: PlayerUpdateMessage | null = livePlayer ?? demoPlayer;

  const [filter, setFilter] = useState<ResourceFilter>(DEFAULT_FILTER);
  const [searchQuery, setSearchQuery] = useState('');
  const [nearestNodeId, setNearestNodeId] = useState<string | null>(null);
  const [nearestDistance, setNearestDistance] = useState(0);
  const [zoomTrigger, setZoomTrigger] = useState(0);

  const activeMapId = player?.isValid ? player.mapId : 1;
  const mapMeta = useMemo(() => findMapMeta(activeMapId), [activeMapId]);
  const resources = useMemo(() => loadResources(activeMapId).nodes, [activeMapId]);

  const nearestName = useMemo(() => {
    if (!nearestNodeId) return null;
    return resources.find((n) => n.id === nearestNodeId)?.name ?? null;
  }, [nearestNodeId, resources]);

  const handleNearestChange = useCallback((id: string | null, distance: number) => {
    setNearestNodeId(id);
    setNearestDistance(distance);
  }, []);

  const handleZoomNearest = () => setZoomTrigger((t) => t + 1);

  return (
    <div className="app-layout">
      <Sidebar
        filter={filter}
        onFilterChange={setFilter}
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
        connected={connected}
        status={status}
        lastError={lastError}
        nearestName={nearestName}
        nearestDistance={nearestDistance}
        resourceCount={resources.length}
        onZoomNearest={handleZoomNearest}
        offlineDemo={offlineDemo}
        onOfflineDemoChange={setOfflineDemo}
        isDemoActive={!!demoPlayer && !connected}
      />
      <main className="map-container">
        <ErrorBoundary label="Map">
          <Suspense fallback={
            <div style={{ padding: 24, color: '#888' }}>Loading map…</div>
          }>
            <MapView
              key={`${activeMapId}-${zoomTrigger}`}
              mapMeta={mapMeta}
              player={player}
              resources={resources}
              filter={filter}
              searchQuery={searchQuery}
              nearestNodeId={nearestNodeId}
              onNearestChange={handleNearestChange}
            />
          </Suspense>
        </ErrorBoundary>
      </main>
      <style>{`
        .app-layout {
          display: flex;
          height: 100vh;
        }
        .map-container {
          flex: 1;
          position: relative;
        }
      `}</style>
    </div>
  );
}
