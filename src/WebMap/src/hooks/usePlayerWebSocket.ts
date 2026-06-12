import { useCallback, useEffect, useRef, useState } from 'react';
import type { PlayerUpdateMessage, ServerStatusMessage, WebSocketMessage } from '../types';
import { WS_URL } from '../config';

interface UsePlayerWebSocketResult {
  player: PlayerUpdateMessage | null;
  status: ServerStatusMessage | null;
  connected: boolean;
  lastError: string | null;
}

export function usePlayerWebSocket(): UsePlayerWebSocketResult {
  const [player, setPlayer] = useState<PlayerUpdateMessage | null>(null);
  const [status, setStatus] = useState<ServerStatusMessage | null>(null);
  const [connected, setConnected] = useState(false);
  const [lastError, setLastError] = useState<string | null>(null);
  const wsRef = useRef<WebSocket | null>(null);
  const reconnectTimer = useRef<number>();

  const connect = useCallback(() => {
    if (wsRef.current?.readyState === WebSocket.OPEN) return;

    try {
      const ws = new WebSocket(WS_URL);
      wsRef.current = ws;

      ws.onopen = () => {
        setConnected(true);
        setLastError(null);
      };

      ws.onmessage = (event) => {
        try {
          const msg = JSON.parse(event.data as string) as WebSocketMessage;
          if (msg.type === 'player_update') {
            setPlayer(msg);
          } else if (msg.type === 'server_status') {
            setStatus(msg);
          }
        } catch {
          setLastError('Malformed message from butler. Classic.');
        }
      };

      ws.onclose = () => {
        setConnected(false);
        reconnectTimer.current = window.setTimeout(connect, 3000);
      };

      ws.onerror = () => {
        setLastError('WebSocket error — is PW Companion running?');
        setConnected(false);
      };
    } catch {
      setLastError('Could not connect to companion WebSocket.');
      reconnectTimer.current = window.setTimeout(connect, 3000);
    }
  }, []);

  useEffect(() => {
    connect();
    return () => {
      window.clearTimeout(reconnectTimer.current);
      wsRef.current?.close();
    };
  }, [connect]);

  return { player, status, connected, lastError };
}
