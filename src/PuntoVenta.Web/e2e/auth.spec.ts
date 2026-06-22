import { test, expect } from '@playwright/test';

test.describe('Auth', () => {
  test('login page carga', async ({ page }) => {
    await page.goto('/login');
    await expect(page.getByRole('textbox', { name: /usuario/i })).toBeVisible();
    await expect(page.getByRole('textbox', { name: /contraseña/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /entrar al panel/i })).toBeVisible();
  });

  test('login con credenciales invalidas muestra error', async ({ page }) => {
    await page.goto('/login');
    await page.getByRole('textbox', { name: /usuario/i }).fill('invalido');
    await page.getByRole('textbox', { name: /contraseña/i }).fill('invalido');
    await page.getByRole('button', { name: /entrar al panel/i }).click();
    await expect(page.getByRole('alert')).toBeVisible();
  });
});
