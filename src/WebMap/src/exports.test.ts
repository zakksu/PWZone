import { describe, expect, it } from 'vitest';

/**
 * Catches broken named/default exports before the browser loads (e.g. Sidebar import errors).
 */
describe('module exports', () => {
  it('Sidebar is exported', async () => {
    const mod = await import('./components/Sidebar');
    expect(mod.default).toBeTypeOf('function');
    expect(mod.Sidebar).toBeTypeOf('function');
  });

  it('MapView is exported as a named function', async () => {
    const mod = await import('./components/MapView');
    expect(mod.MapView).toBeTypeOf('function');
  });

  it('ErrorBoundary is exported as a class', async () => {
    const mod = await import('./components/ErrorBoundary');
    expect(mod.ErrorBoundary).toBeTypeOf('function');
  });

  it('App has a default export', async () => {
    const mod = await import('./App');
    expect(mod.default).toBeTypeOf('function');
  });
});
