/** Base URL for static assets (GitHub Pages subpath or custom domain). */
export function assetUrl(path: string): string {
  const base = import.meta.env.BASE_URL ?? '/';
  const clean = path.replace(/^\//, '');
  return `${base}${clean}`;
}

/** WebSocket URL for local PW Companion desktop app. */
export const WS_URL =
  import.meta.env.VITE_WS_URL ?? 'ws://127.0.0.1:17847/';

export const IS_GITHUB_PAGES =
  import.meta.env.VITE_GITHUB_PAGES === 'true' ||
  (typeof window !== 'undefined' &&
    !window.location.hostname.includes('localhost') &&
    !window.location.hostname.includes('127.0.0.1'));
