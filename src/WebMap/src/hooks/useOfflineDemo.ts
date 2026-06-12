import { useEffect, useState } from 'react';
import type { PlayerUpdateMessage } from '../types';

/** Sample gathering circuit — matches map_1.json nodes. */
const DEMO_WAYPOINTS: Array<{ x: number; z: number }> = [
  { x: 512, z: 768 },
  { x: 320, z: 640 },
  { x: 200, z: 400 },
  { x: 600, z: 500 },
  { x: 680, z: 320 },
  { x: 768, z: 256 },
];

const SPEED = 4;
const TICK_MS = 100;

/**
 * Client-side demo player — works without desktop companion or game.
 */
export function useOfflineDemo(enabled: boolean): PlayerUpdateMessage | null {
  const [player, setPlayer] = useState<PlayerUpdateMessage | null>(null);

  useEffect(() => {
    if (!enabled) {
      setPlayer(null);
      return;
    }

    let x = DEMO_WAYPOINTS[0].x;
    let z = DEMO_WAYPOINTS[0].z;
    let waypointIndex = 0;

    const id = window.setInterval(() => {
      const target = DEMO_WAYPOINTS[waypointIndex];
      const dx = target.x - x;
      const dz = target.z - z;
      const dist = Math.sqrt(dx * dx + dz * dz);

      let facing = 0;
      if (dist <= SPEED) {
        x = target.x;
        z = target.z;
        waypointIndex = (waypointIndex + 1) % DEMO_WAYPOINTS.length;
        const next = DEMO_WAYPOINTS[waypointIndex];
        facing = Math.atan2(next.x - x, next.z - z);
      } else {
        x += (dx / dist) * SPEED;
        z += (dz / dist) * SPEED;
        facing = Math.atan2(dx, dz);
      }

      setPlayer({
        type: 'player_update',
        x,
        y: 10,
        z,
        mapId: 1,
        facing,
        timestamp: Date.now(),
        isValid: true,
        quip: 'Offline demo — no game, no companion, still farming somehow.',
      });
    }, TICK_MS);

    return () => window.clearInterval(id);
  }, [enabled]);

  return player;
}
