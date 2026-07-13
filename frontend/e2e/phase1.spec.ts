import { expect, test } from '@playwright/test'

const workspaceId = '22222222-2222-2222-2222-222222222222'
const shopId = '33333333-3333-3333-3333-333333333333'
const productId = '11111111-2222-3333-4444-555555555555'
const product = {
  id: productId,
  externalListingId: '1001',
  name: 'Dino Wall Hook',
  description: 'Ready to print.',
  etsyPrice: 18.5,
  imageUrl: null,
  isActive: true,
  updatedAt: '2026-05-20T10:30:00.000Z',
}

test.beforeEach(async ({ page }) => {
  let uploaded = false

  await page.route('**/auth/me', async (route) => {
    await route.fulfill({
      json: {
        user: { id: '44444444-4444-4444-4444-444444444444', email: 'newman@example.com', displayName: 'Newman Zone' },
        workspaces: [{ id: workspaceId, name: 'Newman Zone', role: 'Owner' }],
      },
    })
  })

  await page.route(`**/workspaces/${workspaceId}/shops`, async (route) => {
    await route.fulfill({
      json: {
        shops: [{
          id: shopId,
          externalId: '123456',
          shopName: 'Newman Zone',
          lastSyncAt: '2026-05-21T00:00:00.000Z',
        }],
      },
    })
  })

  await page.route(`**/workspaces/${workspaceId}/shops/${shopId}/sync`, async (route) => {
    await route.fulfill({ json: { imported: 0, updated: 1, total: 1, syncedAt: '2026-05-21T00:00:00.000Z' } })
  })

  await page.route(`**/workspaces/${workspaceId}/products`, async (route) => {
    await route.fulfill({ json: { products: [product] } })
  })

  await page.route(`**/workspaces/${workspaceId}/products/${productId}`, async (route) => {
    await route.fulfill({ json: product })
  })

  await page.route(`**/workspaces/${workspaceId}/products/${productId}/files`, async (route) => {
    if (route.request().method() === 'POST') {
      uploaded = true
      await route.fulfill({
        status: 201,
        json: {
          id: '22222222-3333-4444-5555-666666666666',
          productId,
          fileName: 'dino-hook.3mf',
          fileType: '.3mf',
          fileSizeBytes: 4096,
          versionNumber: 1,
          uploadedAt: '2026-05-21T00:00:00.000Z',
        },
      })
      return
    }

    await route.fulfill({
      json: {
        files: uploaded
          ? [{
              id: '22222222-3333-4444-5555-666666666666',
              productId,
              fileName: 'dino-hook.3mf',
              fileType: '.3mf',
              fileSizeBytes: 4096,
              versionNumber: 1,
              uploadedAt: '2026-05-21T00:00:00.000Z',
            }]
          : [],
      },
    })
  })
})

test('phase one Etsy listing and file workflow is usable', async ({ page }) => {
  await page.goto('/dashboard')
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible()
  await expect(page.getByText('Newman Zone is connected.')).toBeVisible()
  await expect(page.getByRole('link', { name: 'Dino Wall Hook' })).toBeVisible()

  await page.getByRole('link', { name: /Products/i }).click()
  await expect(page.getByRole('heading', { name: 'Products' })).toBeVisible()
  await page.getByRole('button', { name: /Sync Etsy/i }).click()
  await expect(page.getByText('Synced 1 listings')).toBeVisible()

  await page.getByRole('link', { name: 'Dino Wall Hook' }).click()
  await expect(page.getByRole('heading', { name: 'Dino Wall Hook' })).toBeVisible()
  await expect(page.getByText('Needs source file')).toBeVisible()

  await page.getByLabel('Choose 3MF or STL file').setInputFiles({
    name: 'dino-hook.3mf',
    mimeType: 'application/octet-stream',
    buffer: Buffer.from('mock 3mf content'),
  })
  await page.getByRole('button', { name: 'Upload file' }).click()
  await expect(page.getByText('dino-hook.3mf uploaded.')).toBeVisible()
  await expect(page.getByRole('link', { name: /Download/i })).toBeVisible()
})
