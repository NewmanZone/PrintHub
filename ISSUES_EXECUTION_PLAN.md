# Issue Execution Plan

## Updated By OpenClaw PM

Generated: 2026-07-01T16:51:30Z  
Repo: NewmanZone/PrintHub  
Planning document: `ISSUES_EXECUTION_PLAN.md`  
Baseline snapshot time: 2026-07-01T16:50:43Z

## Phase 1 Goal

Phase 1 is the shared Etsy file-preparation workspace:

1. Sign in with OAuth.
2. Create or select a workspace.
3. Connect one Etsy shop.
4. Invite a contributor.
5. Import or create products.
6. Attach versioned STL/3MF files to products and parts.
7. Prepare each Etsy order into the right downloadable file bundle and manifest.
8. Download the bundle, print manually, and mark the work complete.

Bambu, OctoEverywhere, direct printer submission, live printer status, automatic printer queues, inventory forecasting, billing, and multi-shop subscriptions are later-phase work. They must stay out of the active Phase 1 plan unless the owner explicitly re-scopes the product.

## Current Implementation Status

The repo documentation now describes a focused Phase 1 product: OAuth-only shared workspace, Etsy shop connection, product/file library, order sync, and manual preparation bundles. The active implementation is behind that documented end goal.

Current baseline state:

- Local repo state is clean.
- Active branch: `feat/issue-9-queue-planning`
- Active PR: `#30 Draft: Package validated PrintHub backend and queue work`
- PR #30 merge state: clean
- PR #30 head: `8732a873c6620a3c6b315688ab2376ba513c7f72`
- Latest CI on PR #30 head: passing
- Branch is 26 commits ahead of `main`, 0 behind.
- No branch-health path coverage blockers were detected for the changed files.
- Open GitHub issues `#5-#19` are stale relative to this plan.

Implemented or partially implemented on PR #30, not yet merged to `main`:

- Etsy shop/service plumbing.
- Etsy API service and fake Etsy service.
- Etsy configuration.
- Shop service.
- Token encryption service.
- Shop controller endpoints.
- Dependency injection coverage.
- Unit tests for Etsy API service, shop service, shops controller, token encryption, and DI resolution.

Not yet complete for the documented Phase 1 end goal:

- OAuth-only current-user bootstrap endpoint.
- Workspace creation, selection, membership, roles, and authorization enforcement.
- Contributor invite flow.
- Persistent Phase 1 domain model across users, workspaces, shops, products, parts, files, orders, and bundles.
- Etsy listing sync and Etsy order sync as a full workspace-scoped workflow.
- File storage, upload, versioning, signed download, deletion, and purge controls.
- Product-to-part-to-current-file mapping APIs.
- Preparation bundle generation, manifest creation, download, completion marking, and audit trail.
- Authenticated React frontend flows backed by real APIs.
- Phase 1 end-to-end tests and manual UI verification.

## Documentation Divergences

| Area | Classification | Status | Required Action |
| --- | --- | --- | --- |
| Phase 1 excludes Bambu, OctoEverywhere, direct print submission, live printer status, and automatic printer queues. | Intended requirement | README and design docs now consistently point Phase 1 toward manual bundles. | Keep this constraint in every issue and PR review. Do not accept printer execution work into Phase 1. |
| OAuth-only authentication with no PrintHub password registration/login/reset. | Intended requirement | Documented in README, API design, data model, and security docs. | Implement `/auth/me`, JWT validation, and profile bootstrap. Reject password-based auth additions. |
| Shared workspace with contributors is central to Phase 1. | Intended requirement | Documented, but not implemented yet. | Prioritize workspace data model and authorization before dependent domain work. |
| Source STL/3MF retention by default with user-controlled delete/purge. | Intended requirement | Documented, but not implemented yet. | Include retention, delete, purge, and signed URL behavior in file-storage issue acceptance criteria. |
| Open issues `#5-#19` still reflect older broad scope and include printer/mock UI/inventory work. | Unintended divergence | GitHub issue queue is stale relative to the documented Phase 1 issue plan. | Replace, close, or retarget stale issues after owner approval. |
| PR #30 title says backend and queue work, but changed paths are mostly Etsy shop/service/token foundation. | Unintended divergence | PR scope appears narrower than title/body imply. | Review PR #30, then retitle/body to match actual scope before marking ready. |
| `DESIGN/dotnet-structure.md` describes `src/PrintHub.API` and split test projects, while repo snapshot includes both `src/PrintHub.Api` and `src/PrintHub.API`, plus `tests/PrintHub.Tests`. | Unintended divergence | Naming/test layout drift may confuse agents and future PRs. | Decide canonical casing/layout, then align docs and project structure before broad feature work. |
| README architecture block is empty/truncated. | Unintended divergence | README lacks a useful architecture diagram. | Fill or remove the empty block during docs cleanup. |
| Historical branches `fix/issue-2`, `fix/issue-3`, and `fix/issue-14` remain open. | Needs owner decision | They may be obsolete, but branch deletion needs approval. | Ask owner whether to prune after confirming no useful work remains. |
| Repo config note says local workspace is dirty, but latest local snapshot says clean. | Needs owner decision | Baseline evidence indicates the note is stale. | Update PM metadata if that note is still maintained elsewhere. |

## Active Blockers And Gates

Normal Phase 1 feature work should wait on these first:

- [ ] Resolve PR #30 scope gate.
  - Confirm the diff is intentionally limited to Etsy/shop/token backend foundation.
  - Retitle and update the PR body if it is not actual queue work.
  - Confirm `dotnet test PrintHub.sln` and `pytest` pass locally or explain why a check is not applicable.
  - Confirm latest GitHub CI remains green on head `8732a873c6620a3c6b315688ab2376ba513c7f72`.
  - Owner approval is required before marking PR #30 ready or merging.

- [ ] Resolve .NET structure naming drift.
  - Choose canonical API project casing: current implementation appears to use `src/PrintHub.Api`, while docs mention `src/PrintHub.API`.
  - Decide whether `tests/PrintHub.Tests` remains the Phase 1 test project or whether split test projects are required later.
  - Update docs and solution/project references to one convention.

- [ ] Reconcile GitHub issues with this plan.
  - Stale issues `#5-#19` should be closed, retargeted, or replaced with small Phase 1 issues.
  - Owner approval is required before rewriting the live issue queue.
  - Printer, inventory, and broad mock-frontend issues should move to later-phase tracking or be closed as superseded.

## Immediate Next PM Focus

Next 2-hour cycle:

- [ ] Review PR #30 against Phase 1 boundaries and prepare it for an owner decision.
- [ ] Produce an updated PR title/body that accurately describes Etsy shop connection, token encryption, service wiring, DI, and tests.
- [ ] Verify latest CI and local test status.
- [ ] Identify whether PR #30 should be marked ready for review, left as draft for more narrowing, or split.
- [ ] Draft the issue reconciliation proposal for stale issues `#5-#19`, but do not mutate issues without owner approval.

Expected proof at end of cycle:

- PR #30 scope summary with changed modules and non-goals.
- Test evidence: `dotnet test PrintHub.sln`, `pytest`, and GitHub CI status.
- Recommendation: ready-for-review, needs edits, split, or close.
- Proposed issue action list for `#5-#19`.

## Prioritized Phase 1 Work Items

### P0: PR And Repo Structure Stabilization

- [ ] Review and scope PR #30.
- [ ] Retitle/body PR #30 to match actual implementation.
- [ ] Confirm no temporary artifacts remain.
- [ ] Confirm no Phase 1 boundary violations: no Bambu, OctoEverywhere, direct print submission, live printer status, or automatic printer queue dependency.
- [ ] Resolve `PrintHub.Api` versus `PrintHub.API` documentation/project naming drift.
- [ ] Fix README architecture block.

Acceptance criteria:

- PR #30 title/body match changed paths.
- Latest CI is green.
- Local verification is recorded or explicitly waived with reason.
- Documentation uses one backend project naming convention.
- README no longer has an empty architecture block.

Verification proof:

- PR link and head SHA.
- CI run conclusion.
- Command output summaries for `dotnet test PrintHub.sln` and `pytest`.
- Diff summary for docs/project naming cleanup.

### P1: Backend Phase 1 Data Model And Workspace Authorization

- [ ] Implement core entities for User, Workspace, WorkspaceMember, Shop, Product, Part, ProductPart, PrintFile, PrintFileVersion, EtsyOrder, EtsyOrderItem, PreparationBundle, PreparationBundleItem, and AuditEvent as needed for Phase 1.
- [ ] Implement repository interfaces and persistence foundation.
- [ ] Implement current-user context abstraction.
- [ ] Implement workspace authorization service.
- [ ] Enforce workspace membership and role checks on protected endpoints.
- [ ] Add tests for owner, contributor, viewer/read-only if retained, non-member denied, removed member denied, and missing workspace denied.

Acceptance criteria:

- Protected workspace APIs validate bearer identity, resolve current user, verify active membership, and enforce role permissions.
- No password auth fields or endpoints are introduced.
- Domain model supports one Etsy shop per workspace for Phase 1.
- Tests cover successful authorization and denial paths.

Verification proof:

- Unit/integration test names and results.
- API examples showing `403` for non-members.
- Model/repository diff summary.

### P1: OAuth-Only Auth And User Profile Bootstrap

- [ ] Add JWT bearer validation configuration.
- [ ] Implement `GET /auth/me`.
- [ ] Bootstrap a user profile on first valid OAuth sign-in.
- [ ] Return user profile and workspace memberships.
- [ ] Implement `POST /auth/logout` only if API-side session state exists.
- [ ] Add tests for first sign-in, returning sign-in, missing token, invalid token, and workspace list response shape.

Acceptance criteria:

- API has no password registration, password login, password reset, or password hash storage.
- `/auth/me` matches documented response shape or the docs are deliberately updated.
- Auth behavior can be configured for local test/dev without real production secrets.

Verification proof:

- Auth tests passing.
- Manual request examples for authenticated and unauthenticated calls.
- Configuration documentation without secrets.

### P1: Workspace Members And Contributor Invites

- [ ] Implement workspace member list endpoint.
- [ ] Implement invite creation.
- [ ] Implement invite accept flow or documented Phase 1 manual acceptance fallback.
- [ ] Implement member role updates/removal for owners.
- [ ] Add audit events for invite and membership changes.
- [ ] Add tests for owner-only invite management and contributor access boundaries.

Acceptance criteria:

- Owners can invite contributors.
- Contributors can access allowed workspace production data.
- Removed members lose access.
- Pending invites are visible to owners.

Verification proof:

- API tests for invite/member flows.
- Manual API examples.
- Audit event test or fixture evidence.

### P1: Etsy Connection, Listing Sync, And Order Sync

- [ ] Build on PR #30 Etsy service foundation after merge.
- [ ] Connect one Etsy shop per workspace.
- [ ] Store encrypted tokens and token metadata.
- [ ] Support token refresh/revocation behavior.
- [ ] Import Etsy listings into workspace product candidates.
- [ ] Sync Etsy orders with personalization/customization fields.
- [ ] Preserve Etsy order context needed for manual preparation.

Acceptance criteria:

- Shop connection is workspace-scoped and owner-managed.
- Tokens are encrypted at rest.
- Listing sync creates or updates product candidates without clobbering user-managed mappings.
- Order sync captures quantities, variations, personalization, buyer-facing identifiers needed for preparation, and sync timestamps.
- Failed Etsy calls produce actionable errors without leaking secrets.

Verification proof:

- Unit/integration tests for token handling and sync mapping.
- Fake Etsy service fixtures for listings/orders.
- Manual sync proof with sanitized sample payloads.

### P1: File Storage, Upload, Versioning, Download, And Purge Controls

- [ ] Implement file storage service abstraction.
- [ ] Accept STL and 3MF uploads with allowlist validation and size limits.
- [ ] Store file metadata by workspace, product/part, and version.
- [ ] Mark current approved version.
- [ ] Provide signed/private download URLs or streamed downloads.
- [ ] Implement user-controlled delete/purge behavior.
- [ ] Add audit events for upload, version change, delete, and purge.

Acceptance criteria:

- Source files are retained by default.
- Files are never publicly readable by default.
- Workspace membership is required for upload/download.
- Delete and purge behavior is explicit and tested.
- Malicious or unsupported file types are rejected.

Verification proof:

- Storage service tests.
- API tests for upload/download/permission denial.
- Manual verification with STL and 3MF sample files.

### P2: Product, Part, And File Mapping API

- [ ] Implement product CRUD or import-confirm workflow.
- [ ] Implement part CRUD.
- [ ] Implement product-to-part mapping with quantities.
- [ ] Link products to Etsy listing IDs.
- [ ] Link parts to current file versions.
- [ ] Support personalization metadata and manual customization instructions.
- [ ] Add tests for mapped, unmapped, personalized, and quantity > 1 products.

Acceptance criteria:

- A product can be created manually or imported from Etsy.
- A product can map to one or more printable parts.
- Each part can resolve to the current approved STL/3MF version.
- Unmapped products are visible as needing setup before bundle generation.

Verification proof:

- API tests for product/part/file mapping.
- Manual request examples.
- Sample response showing setup status.

### P2: Order Preparation Bundle API And Manifest Generation

- [ ] Generate preparation bundles from Etsy orders.
- [ ] Resolve each order item to product, parts, file versions, quantities, and personalization/manual steps.
- [ ] Generate a manifest with order context and file instructions.
- [ ] Support bundle download.
- [ ] Support mark prepared/printed manually.
- [ ] Add tests for complete mappings, missing mappings, multiple quantities, personalized orders, and stale file versions.

Acceptance criteria:

- Bundle generation never requires a printer integration.
- Missing product/file mappings create actionable blocked states.
- Manifest includes enough information for manual printing without exposing unrelated workspace data.
- Completion status is recorded and auditable.

Verification proof:

- Bundle service tests.
- Manifest snapshot or contract tests.
- Manual API flow from synced order to downloaded bundle.

### P2: Frontend App Shell Aligned To Phase 1 Navigation

- [ ] Build authenticated app shell with Phase 1 navigation: dashboard/orders, products/files, workspace/settings.
- [ ] Keep light operations theme.
- [ ] Add loading, empty, error, and permission-denied states.
- [ ] Use mocks only behind a clear dev boundary until real APIs exist.
- [ ] Avoid printer pages and queue/job language that implies direct printer execution.

Acceptance criteria:

- Navigation reflects Phase 1 manual preparation workflow.
- App works at desktop, tablet, and mobile widths without horizontal overflow.
- Keyboard navigation reaches primary controls.
- UI does not include Phase 3 printer execution as active workflow.

Verification proof:

- Frontend tests or Playwright smoke checks.
- Screenshots at approximately 1440px, 768px, and 390px.
- Manual accessibility notes.

### P2: Public Landing Page And OAuth Entry UI

- [ ] Provide a concise public entry page for PrintHub.
- [ ] Implement OAuth sign-in entry.
- [ ] Route signed-in users to workspace selection or current workspace.
- [ ] Route signed-out users away from protected pages.
- [ ] Add error handling for failed auth callback/session state.

Acceptance criteria:

- OAuth is the only sign-in method.
- Protected UI is inaccessible when signed out.
- Auth loading/error states are visible and styled.
- The public page does not overpromise later-phase printer automation.

Verification proof:

- Frontend auth route tests or Playwright flow.
- Manual sign-in/sign-out notes using non-secret dev config.

### P3: Workspace Settings UI For Etsy Connection And Contributors

- [ ] Build workspace settings page.
- [ ] Show Etsy connection state and sync controls.
- [ ] Show member list and pending invites.
- [ ] Support contributor invite flow.
- [ ] Show permission-denied state for non-owner management actions.

Acceptance criteria:

- Owners can manage Etsy connection and contributors.
- Contributors can view appropriate workspace state but cannot perform owner-only changes.
- Failed sync/invite states are recoverable.

Verification proof:

- Component/API integration tests.
- Manual owner/contributor verification notes.
- Screenshots for loading, empty, error, and denied states.

### P3: Product And Part File-Management UI

- [ ] Build products list.
- [ ] Build product detail with Etsy linkage and setup status.
- [ ] Build part mapping UI.
- [ ] Build file upload/versioning/current-version UI.
- [ ] Build delete/purge controls with confirmation.
- [ ] Surface personalization/manual customization requirements.

Acceptance criteria:

- Users can see which products are ready for bundle generation.
- Users can upload STL/3MF files and select current versions.
- Destructive file actions are explicit and permission-checked.
- UI handles empty product library and unmapped imported listings.

Verification proof:

- Playwright or component tests.
- Manual responsive screenshots.
- Sample product setup flow notes.

### P3: Orders And Preparation Bundle UI

- [ ] Build synced orders list.
- [ ] Show setup status for each order item.
- [ ] Build preparation bundle generation flow.
- [ ] Show manifest preview or summary.
- [ ] Support download and manual printed/completed marking.
- [ ] Show clear blocked states for missing product/file mappings.

Acceptance criteria:

- A user can go from synced Etsy order to downloaded preparation bundle without printer integration.
- Personalized order data is visible where needed for manual customization.
- Blocked orders explain exactly what setup is missing.
- Completion state is persisted and visible.

Verification proof:

- End-to-end happy path.
- Blocked-order test.
- Manual desktop/tablet/mobile verification.

### P4: Phase 1 End-To-End Quality Gate

- [ ] Add end-to-end tests for owner setup, contributor access, Etsy sync fixture, product/file setup, order bundle generation, download, and completion.
- [ ] Run accessibility pass on primary pages.
- [ ] Verify no Phase 1 UI depends on direct printer execution.
- [ ] Verify no secrets are committed.
- [ ] Verify docs match implemented API and UI flows.

Acceptance criteria:

- Phase 1 happy path is demonstrable from sign-in through manual bundle completion.
- Tests cover primary failure states.
- Documentation and implementation agree.
- CI is green.

Verification proof:

- CI run.
- E2E test output.
- Manual verification notes.
- Final Phase 1 readiness checklist.

## Proposed Issue Queue

Replace or retarget current stale issues with small PM-ready issues after owner approval:

```yaml
issues:
  101:
    title: Stabilize PR #30 scope and repo structure before Phase 1 feature work
    can_start: true
    blocks: [102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113]
  102:
    title: Backend Phase 1 data model and workspace authorization
    wait_for: [101]
    blocks: [104, 105, 106, 107, 108]
  103:
    title: OAuth-only auth integration and user profile bootstrap
    wait_for: [101]
    blocks: [104, 105, 106, 107, 108]
  104:
    title: Workspace members and contributor invite flow
    wait_for: [102, 103]
    blocks: [110]
  105:
    title: Etsy connection, listing sync, and order sync
    wait_for: [102, 103]
    blocks: [108, 112]
  106:
    title: File storage, upload, versioning, download, and purge controls
    wait_for: [102, 103]
    blocks: [107, 108, 111, 112]
  107:
    title: Product, part, and file mapping API
    wait_for: [102, 105, 106]
    blocks: [108, 111, 112]
  108:
    title: Order preparation bundle API and manifest generation
    wait_for: [105, 106, 107]
    blocks: [112]
  109:
    title: Frontend app shell aligned to Phase 1 navigation
    wait_for: [101]
    blocks: [110, 111, 112, 113]
  110:
    title: Public landing page, OAuth entry, and workspace settings UI
    wait_for: [103, 104, 105, 109]
  111:
    title: Product and part file-management UI
    wait_for: [106, 107, 109]
    blocks: [112]
  112:
    title: Orders and preparation bundle UI
    wait_for: [108, 109, 111]
    blocks: [113]
  113:
    title: Phase 1 end-to-end tests, accessibility pass, and manual verification
    wait_for: [110, 111, 112]
```

## Parallelization Guidance

Can run in parallel after P0 stabilization:

- OAuth profile bootstrap and workspace data model, if user ID and authorization contracts are agreed first.
- Frontend app shell with mocked API boundaries while backend foundations are underway.
- Etsy sync and file storage after workspace authorization exists.
- Contributor settings UI and product/file UI after app shell and backend contracts exist.

Hard waits:

- Product/part/file mapping waits for workspace authorization, Etsy listing contracts, and file versioning.
- Bundle generation waits for order sync, product mappings, and file versioning.
- Orders UI waits for bundle API and product/file UI contracts.
- End-to-end testing waits for all user-facing Phase 1 flows.

## Manual Verification Gate

Every implementation PR must include manual verification notes.

For backend PRs, verify at minimum:

- `dotnet test PrintHub.sln`
- `pytest` if Python tooling remains part of repo checks
- Authenticated success path
- Unauthenticated failure path
- Workspace non-member denied path
- Role-based permission denied path where applicable
- No secrets or real tokens in committed fixtures/logs

For UI PRs, verify at minimum:

- Desktop width around 1440px
- Tablet width around 768px
- Mobile width around 390px
- No horizontal overflow
- Keyboard navigation reaches every primary control
- Empty, loading, error, and permission-denied states are visible and styled
- Light theme remains the default
- No active Phase 1 UI implies direct printer submission or live printer execution

## Later-Phase Bucket

Keep these out of Phase 1 unless the owner explicitly re-scopes:

- Bambu printer integration.
- OctoEverywhere or OctoPrint adapters.
- Direct cloud print submission.
- Live printer telemetry.
- Automatic slicing.
- Build-plate packing.
- Printer queue/job execution.
- Inventory forecasting.
- Cost analytics and reorder intelligence.
- Billing.
- Multi-shop subscriptions.

Existing issues that point at these areas should be closed as superseded, moved to later-phase tracking, or rewritten so they no longer block Phase 1.

## Owner Decisions Needed

- Should PR #30 be marked ready for review after scope/title/body cleanup, or should it remain draft for more changes?
- Is PR #30 allowed to merge once review passes and CI remains green?
- Should OpenClaw replace/retarget stale GitHub issues `#5-#19` with the proposed Phase 1 issue queue?
- Which backend project name is canonical: `PrintHub.Api` or `PrintHub.API`?
- Should tests remain consolidated under `tests/PrintHub.Tests` for Phase 1, or should the repo move toward split test projects?
- Are historical branches `fix/issue-2`, `fix/issue-3`, and `fix/issue-14` safe to prune after inspection?
- Is `Viewer` a required Phase 1 role, or should Phase 1 implement only Owner and Contributor?
