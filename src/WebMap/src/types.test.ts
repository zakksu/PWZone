import { describe, expect, it } from 'vitest';
import { hasValidPosition, worldToMap } from './types';
import type { MapMeta, PlayerUpdateMessage } from './types';

const meta: MapMeta = {
  mapId: 1,
  name: 'Test',
  originX: 0,
  originZ: 0,
  scale: 2,
  imageWidth: 1024,
  imageHeight: 1024,
  tileUrl: '/maps/sample.svg',
};

describe('hasValidPosition', () => {
  it('rejects NaN coordinates even when isValid is true', () => {
    const player = {
      type: 'player_update' as const,
      x: Number.NaN,
      y: 0,
      z: 0,
      mapId: 1,
      timestamp: 0,
      isValid: true,
    };
    expect(hasValidPosition(player)).toBe(false);
  });

  it('accepts finite coordinates', () => {
    const player: PlayerUpdateMessage = {
      type: 'player_update',
      x: 512,
      y: 10,
      z: 768,
      mapId: 1,
      timestamp: 0,
      isValid: true,
    };
    expect(hasValidPosition(player)).toBe(true);
    const [lat, lng] = worldToMap(player.x, player.z, meta);
    expect(Number.isFinite(lat)).toBe(true);
    expect(Number.isFinite(lng)).toBe(true);
  });
});
