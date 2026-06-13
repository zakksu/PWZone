import { useEffect, useMemo, useRef } from 'react';
import { MapContainer, ImageOverlay, Polyline, useMap } from 'react-leaflet';
import L from 'leaflet';
import type { GatheringNode, MapMeta, PlayerUpdateMessage, ResourceFilter } from '../types';
import { hasValidPosition, isSafeLatLng, worldToMap } from '../types';
import { PlayerMarker } from './PlayerMarker';
import { ResourceMarkers } from './ResourceMarkers';

interface MapViewProps {
  mapMeta: MapMeta;
  player: PlayerUpdateMessage | null;
  resources: GatheringNode[];
  filter: ResourceFilter;
  searchQuery: string;
  nearestNodeId: string | null;
  onNearestChange: (id: string | null, distance: number) => void;
}

function MapBounds({ meta }: { meta: MapMeta }) {
  const map = useMap();
  useEffect(() => {
    const bounds = L.latLngBounds(
      [0, 0],
      [meta.imageHeight, meta.imageWidth],
    );
    map.fitBounds(bounds);
    map.setMaxBounds(bounds.pad(0.05));
  }, [map, meta]);
  return null;
}

function ZoomToNearest({
  player,
  meta,
  nearestNode,
}: {
  player: PlayerUpdateMessage | null;
  meta: MapMeta;
  nearestNode: GatheringNode | null;
}) {
  const map = useMap();
  const lastZoom = useRef<string | null>(null);

  useEffect(() => {
    if (!hasValidPosition(player) || !nearestNode) return;
    const key = `${nearestNode.id}-${player.x.toFixed(0)}`;
    if (lastZoom.current === key) return;
    lastZoom.current = key;

    const [pLat, pLng] = worldToMap(player.x, player.z, meta);
    const [nLat, nLng] = worldToMap(nearestNode.x, nearestNode.z, meta);
    map.fitBounds(L.latLngBounds([pLat, pLng], [nLat, nLng]).pad(0.3));
  }, [player, nearestNode, meta, map]);

  return null;
}

export function MapView({
  mapMeta,
  player,
  resources,
  filter,
  searchQuery,
  nearestNodeId,
  onNearestChange,
}: MapViewProps) {
  const filteredResources = useMemo(() => {
    const q = searchQuery.toLowerCase().trim();
    return resources.filter((r) => {
      if (!filter[r.type as keyof ResourceFilter]) return false;
      if (q && !r.name.toLowerCase().includes(q) && !r.type.includes(q)) return false;
      return true;
    });
  }, [resources, filter, searchQuery]);

  const nearestNode = useMemo(() => {
    if (!hasValidPosition(player) || filteredResources.length === 0)
      return null;

    let best: GatheringNode | null = null;
    let bestDist = Infinity;

    for (const node of filteredResources) {
      const dx = node.x - player.x;
      const dz = node.z - player.z;
      const dist = Math.sqrt(dx * dx + dz * dz);
      if (dist < bestDist) {
        bestDist = dist;
        best = node;
      }
    }

    return best;
  }, [player, filteredResources]);

  useEffect(() => {
    if (!nearestNode) {
      onNearestChange(null, 0);
      return;
    }

    const dx = nearestNode.x - (player?.x ?? 0);
    const dz = nearestNode.z - (player?.z ?? 0);
    onNearestChange(nearestNode.id, Math.sqrt(dx * dx + dz * dz));
  }, [nearestNode, player, onNearestChange]);

  const linePositions = useMemo(() => {
    if (!hasValidPosition(player) || !nearestNode) return [];
    const [pLat, pLng] = worldToMap(player!.x, player!.z, mapMeta);
    const [nLat, nLng] = worldToMap(nearestNode.x, nearestNode.z, mapMeta);
    if (!isSafeLatLng(pLat, pLng) || !isSafeLatLng(nLat, nLng)) return [];
    return [[pLat, pLng] as [number, number], [nLat, nLng] as [number, number]];
  }, [player, nearestNode, mapMeta]);

  const bounds: L.LatLngBoundsExpression = [
    [0, 0],
    [mapMeta.imageHeight, mapMeta.imageWidth],
  ];

  return (
    <MapContainer
      crs={L.CRS.Simple}
      bounds={bounds}
      style={{ height: '100%', width: '100%' }}
      zoomControl={true}
      attributionControl={false}
    >
      <MapBounds meta={mapMeta} />
      <ImageOverlay url={mapMeta.tileUrl} bounds={bounds} />
      <ResourceMarkers
        nodes={filteredResources}
        mapMeta={mapMeta}
        highlightId={nearestNodeId}
      />
      {hasValidPosition(player) && (
        <PlayerMarker player={player!} mapMeta={mapMeta} />
      )}
      {linePositions.length === 2 && (
        <Polyline
          positions={linePositions}
          pathOptions={{ className: 'nearest-line', color: '#e94560', dashArray: '6 4' }}
        />
      )}
      <ZoomToNearest player={player} meta={mapMeta} nearestNode={nearestNode} />
    </MapContainer>
  );
}
