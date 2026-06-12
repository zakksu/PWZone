import { vi } from 'vitest';

/** Minimal WebSocket mock — companion is not running in unit tests. */
class MockWebSocket {
  static readonly CONNECTING = 0;
  static readonly OPEN = 1;
  static readonly CLOSING = 2;
  static readonly CLOSED = 3;

  readyState = MockWebSocket.CONNECTING;
  onopen: (() => void) | null = null;
  onclose: (() => void) | null = null;
  onerror: (() => void) | null = null;
  onmessage: ((event: { data: string }) => void) | null = null;

  constructor(_url: string) {
    queueMicrotask(() => {
      this.readyState = MockWebSocket.CLOSED;
      this.onclose?.();
    });
  }

  close() {
    this.readyState = MockWebSocket.CLOSED;
  }

  send() {
    /* noop */
  }
}

vi.stubGlobal('WebSocket', MockWebSocket);

vi.mock('leaflet', () => ({
  default: {
    latLngBounds: vi.fn(() => ({})),
    divIcon: vi.fn(() => ({})),
  },
}));

vi.mock('react-leaflet', () => ({
  MapContainer: ({ children }: { children?: unknown }) => children,
  ImageOverlay: () => null,
  Polyline: () => null,
  useMap: () => ({
    fitBounds: vi.fn(),
    setView: vi.fn(),
  }),
}));
