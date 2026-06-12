import { Marker, Popup } from 'react-leaflet';
import L from 'leaflet';
import type { PlayerUpdateMessage, MapMeta } from '../types';
import { worldToMap } from '../types';

interface PlayerMarkerProps {
  player: PlayerUpdateMessage;
  mapMeta: MapMeta;
}

export function PlayerMarker({ player, mapMeta }: PlayerMarkerProps) {
  const [lat, lng] = worldToMap(player.x, player.z, mapMeta);
  const rotation = player.facing != null ? (player.facing * 180) / Math.PI : 0;

  const icon = L.divIcon({
    className: 'player-marker',
    html: `<div class="player-arrow" style="transform: rotate(${rotation}deg)"></div>`,
    iconSize: [24, 24],
    iconAnchor: [12, 12],
  });

  return (
    <Marker position={[lat, lng]} icon={icon}>
      <Popup>
        <strong>You</strong>
        <br />
        X: {player.x.toFixed(1)} Z: {player.z.toFixed(1)}
        <br />
        Map: {player.mapId}
        {player.quip && (
          <>
            <br />
            <em>{player.quip}</em>
          </>
        )}
      </Popup>
    </Marker>
  );
}
