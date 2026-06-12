import { Marker, Popup } from 'react-leaflet';
import L from 'leaflet';
import type { GatheringNode, MapMeta } from '../types';
import { worldToMap, formatRespawn, randomQuip } from '../types';

interface ResourceMarkersProps {
  nodes: GatheringNode[];
  mapMeta: MapMeta;
  highlightId: string | null;
}

function createResourceIcon(type: string, highlighted: boolean): L.DivIcon {
  const style = highlighted
    ? 'box-shadow:0 0 10px #e94560;transform:scale(1.4)'
    : '';
  return L.divIcon({
    className: 'resource-marker-wrapper',
    html: `<div class="resource-marker ${type}" style="${style}"></div>`,
    iconSize: highlighted ? [16, 16] : [12, 12],
    iconAnchor: highlighted ? [8, 8] : [6, 6],
  });
}

export function ResourceMarkers({ nodes, mapMeta, highlightId }: ResourceMarkersProps) {
  return (
    <>
      {nodes.map((node) => {
        const [lat, lng] = worldToMap(node.x, node.z, mapMeta);
        const highlighted = node.id === highlightId;

        return (
          <Marker
            key={node.id}
            position={[lat, lng]}
            icon={createResourceIcon(node.type, highlighted)}
          >
            <Popup>
              <strong>{node.name}</strong>
              <br />
              Type: {node.type} · Lv {node.level}
              <br />
              Respawn: ~{formatRespawn(node.respawnSeconds)}
              {node.notes && (
                <>
                  <br />
                  {node.notes}
                </>
              )}
              <br />
              <em style={{ fontSize: '0.85em', opacity: 0.7 }}>{randomQuip()}</em>
            </Popup>
          </Marker>
        );
      })}
    </>
  );
}
