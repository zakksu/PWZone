export interface PlayerUpdateMessage {
  type: 'player_update';
  x: number;
  y: number;
  z: number;
  mapId: number;
  facing?: number;
  timestamp: number;
  isValid: boolean;
  quip?: string;
}

export interface ServerStatusMessage {
  type: 'server_status';
  attached: boolean;
  processName: string;
  processId?: number;
  offsetVersion: string;
  message: string;
}

export type WebSocketMessage = PlayerUpdateMessage | ServerStatusMessage;

export interface MapMeta {
  mapId: number;
  name: string;
  originX: number;
  originZ: number;
  scale: number;
  imageWidth: number;
  imageHeight: number;
  tileUrl: string;
}

export interface GatheringNode {
  id: string;
  name: string;
  type: 'herb' | 'ore' | 'wood' | 'plant' | string;
  level: number;
  x: number;
  z: number;
  respawnSeconds: number;
  notes?: string;
}

export interface GatheringMapData {
  mapId: number;
  mapName: string;
  nodes: GatheringNode[];
}

export interface ResourceFilter {
  herb: boolean;
  ore: boolean;
  wood: boolean;
  plant: boolean;
}

export const DEFAULT_FILTER: ResourceFilter = {
  herb: true,
  ore: true,
  wood: true,
  plant: true,
};

export function worldToMap(
  worldX: number,
  worldZ: number,
  meta: MapMeta,
): [number, number] {
  const pixelX = (worldX - meta.originX) / meta.scale;
  const pixelY = (worldZ - meta.originZ) / meta.scale;
  const lat = meta.imageHeight - pixelY;
  const lng = pixelX;
  return [lat, lng];
}

/** True when player coords are safe to pass to Leaflet (finite, flagged valid). */
export function hasValidPosition(player: PlayerUpdateMessage | null | undefined): boolean {
  if (!player?.isValid) return false;
  return (
    Number.isFinite(player.x) &&
    Number.isFinite(player.y) &&
    Number.isFinite(player.z)
  );
}

export function isSafeLatLng(lat: number, lng: number): boolean {
  return Number.isFinite(lat) && Number.isFinite(lng);
}

export function distance2D(x1: number, z1: number, x2: number, z2: number): number {
  const dx = x2 - x1;
  const dz = z2 - z1;
  return Math.sqrt(dx * dx + dz * dz);
}

export function formatRespawn(seconds: number): string {
  if (seconds < 60) return `${seconds}s`;
  const m = Math.floor(seconds / 60);
  const s = seconds % 60;
  return s > 0 ? `${m}m ${s}s` : `${m}m`;
}

export const BUTLER_QUIPS = [
  'Another herb. Groundbreaking.',
  'The butler sees all. Especially that ore over there.',
  'You could walk there. Or you could keep standing still. Your call.',
  'Respawn timers are estimates. Like my enthusiasm.',
];

export function randomQuip(): string {
  return BUTLER_QUIPS[Math.floor(Math.random() * BUTLER_QUIPS.length)];
}
