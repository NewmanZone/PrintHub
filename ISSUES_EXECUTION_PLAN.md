# Issue Execution Plan

## Goal

Phase 1 is the shared Etsy file-preparation workspace:

1. Sign in with OAuth.
2. Create/select a workspace.
3. Connect one Etsy shop.
4. Invite a contributor.
5. Import or create products.
6. Attach versioned STL/3MF files to products/parts.
7. Prepare each order into the right downloadable file bundle.
8. Download the bundle and mark the work printed manually.

Bambu, OctoEverywhere, direct print submission, live printer status, and automatic printer queues are later-phase work. They must not block Phase 1.

## Issue Shape For OpenClaw/Ollama Agents

Each issue should be self-contained and include:

- Exact goal and non-goals.
- Files or modules expected to change.
- API/data contracts to follow.
- UI states to implement, including loading, empty, error, and permission-denied states.
- Happy-path and edge-case tests required.
- Manual verification steps.
- Dependency note: "Can start now" or "Wait for issue X PR to merge."

Prefer smaller issues with explicit acceptance criteria over broad "build feature" issues.

## Proposed Phase 1 Issues

```yaml
issues:
  101:
    title: Backend Phase 1 data model and workspace authorization
    can_start: true
    blocks: [102, 103, 104, 105, 106]
  102:
    title: OAuth-only auth integration and user profile bootstrap
    can_start: true
    blocks: [103, 104, 105, 106]
  103:
    title: Workspace members and contributor invite flow
    wait_for: [101, 102]
    blocks: [110]
  104:
    title: Etsy connection, listing sync, and order sync
    wait_for: [101, 102]
    blocks: [107, 108]
  105:
    title: File storage, upload, versioning, download, and purge controls
    wait_for: [101, 102]
    blocks: [107, 108]
  106:
    title: Frontend app shell aligned to Phase 1 navigation
    can_start: true
    blocks: [109, 110, 111, 112]
  107:
    title: Product, part, and file mapping API
    wait_for: [101, 104, 105]
    blocks: [108, 111]
  108:
    title: Order preparation bundle API and manifest generation
    wait_for: [104, 105, 107]
    blocks: [112]
  109:
    title: Public landing page and OAuth entry UI
    wait_for: [106]
  110:
    title: Workspace settings UI for Etsy connection and contributors
    wait_for: [103, 104, 106]
  111:
    title: Product and part file-management UI
    wait_for: [106, 107]
    blocks: [112]
  112:
    title: Orders and preparation bundle UI
    wait_for: [106, 108, 111]
    blocks: [113]
  113:
    title: Phase 1 end-to-end tests, accessibility pass, and manual UI verification
    wait_for: [109, 110, 111, 112]
```

## Parallel Groups

```yaml
groups:
  wave_0_can_start_now:
    - 101 # data model/workspace authorization
    - 102 # OAuth profile bootstrap
    - 106 # frontend shell/navigation

  wave_1_after_foundations:
    - 103 # contributors
    - 104 # Etsy sync
    - 105 # file storage/versioning
    - 109 # landing/OAuth entry UI

  wave_2_domain_work:
    - 107 # product/part/file mapping API
    - 110 # settings UI for Etsy and contributors

  wave_3_order_preparation:
    - 108 # bundle API/manifest generation
    - 111 # product/part file UI

  wave_4_integration_and_quality:
    - 112 # orders/bundle UI
    - 113 # E2E, accessibility, manual verification
```

## Hard Waits

- Product/part/file mapping API waits for data model, Etsy sync contracts, and file storage.
- Order bundle generation waits for product mappings and file versioning.
- Orders UI waits for the bundle API and product/file UI contracts.
- End-to-end testing waits for all user-facing Phase 1 flows to exist.

## Can Run In Parallel

- OAuth profile bootstrap and workspace data model can run together if they agree on user id and authorization contracts.
- Frontend shell/navigation can run while backend foundations are being built, using mocks.
- Etsy sync and file storage can run in parallel after workspace authorization exists.
- Contributor settings UI can run in parallel with product/file UI after the app shell exists.

## Manual Verification Gate

Every implementation PR should include manual verification notes. For UI PRs, verify at minimum:

- Desktop width around 1440px.
- Tablet width around 768px.
- Mobile width around 390px.
- No horizontal overflow.
- Keyboard navigation reaches every primary control.
- Empty, loading, error, and permission-denied states are visible and styled.
- Light theme remains the default.

## Later-Phase Issue Bucket

Keep these out of Phase 1 unless the user explicitly re-scopes:

- Bambu/OctoEverywhere printer adapters.
- Direct cloud print submission.
- Live printer telemetry.
- Automatic slicing or build-plate packing.
- Inventory forecasting and reorder intelligence.
- Billing and multi-shop subscriptions.
