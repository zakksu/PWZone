import { test, expect } from '@playwright/test';

/** Errors that are expected when the desktop companion is not running in CI. */
function isBenignConsoleError(text: string): boolean {
  return (
    text.includes('WebSocket') ||
    text.includes('websocket') ||
    text.includes('127.0.0.1:17847')
  );
}

test('production build loads without fatal console errors', async ({ page }) => {
  const errors: string[] = [];

  page.on('console', (msg) => {
    if (msg.type() === 'error') errors.push(msg.text());
  });
  page.on('pageerror', (err) => errors.push(err.message));

  await page.goto('/');

  await expect(page.getByRole('heading', { name: 'PW Companion' })).toBeVisible();
  await expect(page.locator('#root')).not.toBeEmpty();

  const fatal = errors.filter((e) => !isBenignConsoleError(e));
  expect(fatal, `Unexpected console errors:\n${fatal.join('\n')}`).toEqual([]);
});

test('module graph resolves (no blank white screen)', async ({ page }) => {
  await page.goto('/');

  await expect(page.locator('.sidebar')).toBeVisible();
  await expect(page.locator('.map-container')).toBeVisible();
});
