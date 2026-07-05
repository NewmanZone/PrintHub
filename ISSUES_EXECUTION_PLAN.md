# Issue Execution Plan

## Updated By OpenClaw PM

Generated: maintained by OpenClaw PM
Repo: NewmanZone/PrintHub  
Planning document: `ISSUES_EXECUTION_PLAN.md`  
Current planning source: `origin/pm/docs-plan:ISSUES_EXECUTION_PLAN.md`

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

The repo documentation now consistently describes a focused Phase 1 product: OAuth-only auth, shared workspaces, Etsy shop connection, product/file library, Etsy order sync, manual preparation bundles, and completion tracking. The implementation is partially aligned, but still behind the documented end goal.

Current repo state from the baseline snapshot:

- Default branch: `main`
- Open PRs: none
- Local workspace state: clean, on `main`, synced with `origin/main`
- Branch structural blockers: none reported
- Source path structural blockers: none reported
- Open GitHub issues: 12
- Open branch inventory includes old feature/fix/PM branches that need owner-approved inspection before pruning
- Latest normal CI on `main` passed in the supplied snapshot
- Recent `Deploy to Azure` runs on `main` failed repeatedly and need triage or an explicit waiver
- Snapshot mode says `active / autonomous / auto_merge`, but baseline advisory notes require owner confirmation before automation proceeds

Implemented or partially implemented on `main`:

- Etsy shop/service plumbing
- Etsy API service and fake Etsy service
- Etsy configuration
- Shop service
- Token encryption service
- Shop controller endpoints
- Product repository interface and in-memory product repository foundation
- Product sync foundation
- Dependency injection wiring and coverage
- Tests for Etsy API service, shop service, shops controller, auth pipeline behavior, token encryption, and DI resolution
- Project/runtime support changes including `global.json`, project files, API Dockerfile updates, and related CI/deploy plumbing
- Documentation updates aligning Phase 1 away from printer execution and toward manual Etsy file preparation

Not yet complete for the documented Phase 1 end goal:

- OAuth-only current-user bootstrap endpoint
- Workspace creation, selection, membership, roles, and authorization enforcement
- Contributor invite flow
- Persistent Phase 1 domain model across users, workspaces, shops, products, parts, files, orders, and bundles
- Durable persistence beyond in-memory foundations where Phase 1 requires it
- Full Etsy listing sync and Etsy order sync as a workspace-scoped workflow
- File storage, upload, versioning, signed/private download, deletion, and purge controls
- Product-to-part-to-current-file mapping APIs
- Preparation bundle generation, manifest creation, download, completion marking, and audit trail
- Authenticated React frontend flows backed by real APIs
- Phase 1 end-to-end tests and manual UI verification

## Documentation Divergences

| Area | Classification | Current Status | Required Action |
| --- | --- | --- | --- |
| Phase 1 excludes Bambu, OctoEverywhere, direct print submission, live printer status, and automatic printer queues. | Intended requirement | README and design docs now point Phase 1 toward manual bundles. | Keep this constraint in every issue, PR, and review. Do not accept printer execution work into Phase 1. |
| OAuth-only authentication with no PrintHub password registration/login/reset. | Intended requirement | Documented in README, API design, data model, and security docs. | Implement JWT validation, `/auth/me`, and profile bootstrap. Reject password-based auth additions. |
| Shared workspace with contributors is central to Phase 1. | Intended requirement | Documented, but not implemented yet. | Prioritize workspace model and authorization before dependent domain work. |
| Source STL/3MF retention by default with user-controlled delete/purge. | Intended requirement | Documented, but not implemented yet. | Include retention, delete, purge, and signed/private download behavior in file-storage acceptance criteria. |
| Manual preparation bundles replace printer queue execution in Phase 1. | Intended requirement | README and design docs agree. `DESIGN/print-queue.md` is marked later-phase reference. | Keep queue/printer language out of active Phase 1 implementation unless explicitly scoped as bundle preparation. |
| Open PR state in older planning text said PR `#31` was open. | Unintended divergence | Current snapshot reports no open PRs. | Treat PR `#31` as no longer an active blocker; PM cycles should verify live PR state before acting. |
| Open issues `#5-#19` still reflect older broad scope and include printer/mock UI/inventory work. | Unintended divergence | GitHub issue queue is stale relative to the documented Phase 1 queue. | Replace, close, or retarget stale issues after owner approval. |
| README architecture block appears empty/truncated in the excerpt. | Unintended divergence | README lacks a useful architecture diagram in the supplied canonical excerpt. | Fill or remove the empty block during docs cleanup. |
| Repo description says “3D print operations with Etsy integration.” | Needs owner decision | Broader than current Phase 1 but still accurate for long-term product. | Decide whether to narrow it to “shared Etsy file preparation” or leave it as broader roadmap language. |
| Azure deployment workflow is failing while CI passes. | Needs owner decision | Recent deploy runs on `main` failed. | Decide whether Azure deploy is an active Phase 1 blocker now or later release hardening. |
| Historical branches remain without open PRs. | Needs owner decision | Branches may contain obsolete or reusable work. | Inspect and prune only after owner approval. |
| `Viewer` role appears in data model guidance but may be optional for Phase 1. | Needs owner decision | Owner and contributor are clearly required; viewer may add extra scope. | Decide whether Phase 1 includes Viewer or defers it. |
| Snapshot mode indicates autonomous auto-merge, while baseline says owner confirmation is needed. | Needs owner decision | Automation mode is inconsistent with advisory guard. | Owner must confirm operating mode before PM resumes mutation or auto-merge work. |

## Active Blockers And Gates

Normal feature work should start only after these gates are handled or explicitly waived.

- [ ] Confirm operating mode before mutation.
  - Owner must choose advisory, supervised, or autonomous/auto-merge.
  - Do not mutate issue queues, delete branches, or auto-merge until this is confirmed.

- [ ] Triage Azure deploy failure.
  - Capture failing workflow job and step.
  - Determine whether failure is configuration, secret/identity, infrastructure, app startup, or environment drift.
  - Decide whether Azure deploy is a promotion gate for Phase 1 or a later release-hardening track.

- [ ] Reconcile the stale issue queue.
  - Stale issues `#5-#19` should be closed, retargeted, or replaced with small Phase 1 issues.
  - Owner approval is required before rewriting the live issue queue.
  - Printer, inventory, and broad mock-frontend issues should move to later-phase tracking or be closed as superseded.

- [ ] Inspect old branches before pruning.
  - `codex/pm-newmanzone__printhub-20260703t063416z`
  - `codex/pm-newmanzone__printhub-20260703t090148z`
  - `feat/issue-9-queue-planning`
  - `fix/issue-2`
  - `fix/issue-3`
  - `fix/issue-14`
  - `pm/docs-plan`

- [ ] Clean up documentation friction.
  - Fix or remove the empty README architecture block.
  - Ensure docs consistently treat `DESIGN/print-queue.md` and Bambu spike docs as later-phase reference only.
  - Keep planning docs free of stale PR status claims.

## Immediate Next PM Focus

Next 2-hour cycle:

- [ ] Confirm live GitHub state: open PRs, open issues, branch inventory, CI, and deploy status.
- [ ] Confirm owner operating mode: advisory, supervised, or autonomous/auto-merge.
- [ ] Triage the latest Azure deploy failure enough to identify the failing job/step and likely owner decision.
- [ ] Produce owner-facing issue reconciliation actions for issues `#5-#19`.
- [ ] Inspect old branches non-destructively and recommend keep/close/delete actions.
- [ ] Prepare the first implementation slice after stabilization: OAuth-only `/auth/me` bootstrap and current-user contract.
- [ ] If owner approves mutation, create or retarget the first small Phase 1 issues in dependency order.

Expected proof at end of cycle:

- Live PR count and PR blocker summary.
- Live CI and deploy conclusions checked from GitHub.
- Azure failure job/step summary.
- Proposed issue action table for `#5-#19`.
- Branch inspection summary with no destructive actions taken unless approved.
- Recommended first implementation issue with acceptance criteria.

## Prioritized Phase 1 Work Items

### P0: PM Gates, CI/Deploy, Issues, And Repo Planning Stabilization

- [ ] Confirm operating mode with owner.
- [ ] Verify live PR state is still clear before starting implementation work.
- [ ] Verify live `main` CI status.
- [ ] Triage current Azure deploy failure and decide whether it blocks promotion.
- [ ] Fix README architecture block.
- [ ] Prepare issue reconciliation proposal for stale issues `#5-#19`.
- [ ] Inspect old branches and prepare pruning recommendation.
- [ ] Ensure planning docs do not contain stale PR or volatile check claims.

Acceptance criteria:

- Owner-approved operating mode is recorded in PM output.
- `ISSUES_EXECUTION_PLAN.md` on `main` is current enough for recurring PM cycles.
- Azure deploy is either green, explicitly out of scope, or represented by a concrete follow-up issue.
- README no longer has an empty architecture block.
- Issue reconciliation proposal is ready for owner approval.
- Branch pruning recommendation is owner-ready and non-destructive.

Verification proof:

- Live PR/check/deploy state recorded by PM run.
- Azure failure job/step summary.
- Diff summary for docs cleanup.
- Proposed issue action table.
- Branch inspection summary.

### P1: OAuth-Only Auth And User Profile Bootstrap

- [ ] Add JWT bearer validation configuration.
- [ ] Implement `GET /auth/me`.
- [ ] Bootstrap a user profile on first valid OAuth sign-in.
- [ ] Return user profile and workspace memberships.
- [ ] Implement `POST /auth/logout` only if API-side session state exists.
- [ ] Add tests for first sign-in, returning sign-in, missing token, invalid token, and workspace list response shape.

Acceptance criteria:

- API has no password registration, password login, password reset, or password hash storage.
- `/auth/me` matches documented response shape or docs are deliberately updated.
- Auth behavior can be configured for local test/dev without production secrets.
- Protected endpoints can rely on a current-user abstraction.

Verification proof:

- Auth tests passing.
- Manual request examples for authenticated and unauthenticated calls.
- Configuration documentation without secrets.

### P1: Backend Phase 1 Data Model And Workspace Authorization

- [ ] Implement core entities for User, Workspace, WorkspaceMember, Shop, Product, Part, ProductPart, PrintFile, PrintFileVersion, EtsyOrder, EtsyOrderItem, PreparationBundle, PreparationBundleItem, and AuditEvent as needed for Phase 1.
- [ ] Implement repository interfaces and persistence foundation.
- [ ] Replace in-memory-only foundations where durable Phase 1 behavior requires persistence.
- [ ] Implement current-user context abstraction.
- [ ] Implement workspace authorization service.
- [ ] Enforce workspace membership and role checks on protected endpoints.
- [ ] Add tests for owner, contributor, optional viewer/read-only, non-member denied, removed member denied, and missing workspace denied.

Acceptance criteria:

- Protected workspace APIs validate bearer identity, resolve current user, verify active membership, and enforce role permissions.
- No password auth fields or endpoints are introduced.
- Domain model supports one Etsy shop per workspace for Phase 1.
- Tests cover successful authorization and denial paths.

Verification proof:

- Unit/integration test names and results.
- API examples showing `403` for non-members.
- Model/repository diff summary.

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

- [ ] Build on the merged Etsy service and product sync foundation.
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
- [ ] Add tests for mapped, unmapped, personalized, and quantity greater than 1 products.

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
- UI does not include Phase 3 printer execution as an active workflow.

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
- Public page does not overpromise later-phase printer automation.

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
    title: Stabilize planning docs, CI/deploy gates, issue queue, and repo branches before Phase 1 feature work
    can_start: true
    blocks: [102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113]
  102:
    title: OAuth-only auth integration and user profile bootstrap
    wait_for: [101]
    blocks: [103, 104, 105, 106, 107, 108]
  103:
    title: Backend Phase 1 data model and workspace authorization
    wait_for: [101, 102]
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
    wait_for: [103, 105, 106]
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
    wait_for: [102, 104, 105, 109]
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

## Stale Issue Reconciliation Proposal

Do not mutate these issues until the owner approves the rewrite.

| Existing Issue | Proposed Action | Reason |
| --- | --- | --- |
| `#5 Data: implement Cosmos DB containers, tenant model, and repository foundation` | Retarget into issue 103 or close as superseded after replacement. | Too broad and tied to older tenant wording; workspace authorization needs sharper Phase 1 acceptance criteria. |
| `#6 Backend: implement OAuth/B2C authentication and current-user API contract` | Retarget into issue 102. | Still directionally valid, but needs OAuth-only lock and `/auth/me` acceptance criteria. |
| `#7 Backend: implement Etsy shop OAuth connection and listing sync foundation` | Retarget into issue 105 or close as partially covered after confirming merged scope. | Backend foundation exists, but full workspace-scoped listing/order sync remains. |
| `#10 Backend: implement printer adapter contract with Mock and OctoEverywhere/OctoPrint adapters` | Move to later phase or close as superseded. | Printer adapters are explicitly out of Phase 1. |
| `#11 Frontend: build public landing page and demo workspace entry` | Retarget into issue 110. | Keep OAuth entry; avoid demo workspace if it conflicts with real auth/workspace flow. |
| `#12 Frontend: build dashboard and queue planning pages with mock data` | Retarget into issues 109 and 112. | Queue wording is stale; Phase 1 needs manual preparation/order bundle UI. |
| `#13 Frontend: build products, product detail, parts, and file version pages with mock data` | Retarget into issue 111. | Directionally valid but should depend on product/file API contracts. |
| `#14 Frontend: build printers, settings, orders, and jobs workspace pages with mock data` | Split or close as superseded. | Settings/orders remain valid; printers/jobs are later phase. |
| `#16 Backend: implement inventory movements, cost records, alerts, and insights dashboard` | Move to later phase or close as superseded. | Inventory/cost/alerts are not Phase 1 blockers. |
| `#17 Backend: implement personalized Etsy orders sync and queue handoff` | Retarget into issues 105 and 108. | Personalized orders remain critical, but “queue handoff” must become manual preparation bundle generation. |
| `#18 Frontend: wire real API clients, OAuth flow, and authenticated data loading` | Retarget across issues 109, 110, 111, and 112. | Still valid but too broad for one PM-ready issue. |
| `#19 Infra: add Azure deployment, managed identity config, secrets, and operational docs` | Retarget into issue 101 or a separate deploy-hardening issue after triage. | Deploy is failing, but owner must decide whether it blocks Phase 1 feature work. |

## Branch Reconciliation Proposal

Do not delete or force-update branches until the owner approves.

| Branch | Proposed Action | Reason |
| --- | --- | --- |
| `main` | Keep. | Default branch. |
| `pm/docs-plan` | Inspect and delete if fully merged and no active PR depends on it. | Current snapshot reports no open PRs; branch may be stale. |
| `codex/pm-newmanzone__printhub-20260703t063416z` | Inspect diff against `main`; delete if merged/superseded. | Old PM branch without open PR. |
| `codex/pm-newmanzone__printhub-20260703t090148z` | Inspect diff against `main`; delete if merged/superseded. | Old PM branch without open PR. |
| `feat/issue-9-queue-planning` | Inspect only; likely defer or archive. | Queue/planning may conflict with Phase 1 manual bundle scope. |
| `fix/issue-2` | Inspect diff and close/delete if obsolete. | Old fix branch without open PR. |
| `fix/issue-3` | Inspect diff and close/delete if obsolete. | Old fix branch without open PR. |
| `fix/issue-14` | Inspect diff and close/delete if obsolete. | Old fix branch without open PR. |

## Standard Verification Commands

Run before any merge or promotion where applicable:

- [ ] `dotnet test PrintHub.sln`
- [ ] `pytest`
- [ ] Frontend install/build/test/e2e checks from `frontend` when frontend files change
- [ ] GitHub CI on the target branch
- [ ] Azure deploy workflow, or documented waiver if deployment is not currently a required gate
- [ ] Manual product-flow smoke test once implemented: sign in, workspace, Etsy connection, product/file mapping, order import, bundle prep/download

## Later-Phase Work Explicitly Out Of Active Plan

Keep these out of Phase 1 implementation unless the owner explicitly changes scope:

- Bambu integration
- OctoEverywhere/OctoPrint adapters
- Direct printer submission
- Live printer status
- Automatic printer queues
- Printer job history as a required workflow
- Inventory forecasting
- Cost dashboards
- Low-stock alerts
- Billing and subscriptions
- Multi-shop subscriptions

## Open Questions For Owner

1. Should PM continue in advisory mode, supervised mode, or autonomous/auto-merge mode?
2. Does the current Azure deploy failure block all promotion, or can feature work continue while deploy is repaired separately?
3. Should the repo description be narrowed to the Phase 1 file-prep product, or remain broader for the long-term roadmap?
4. May OpenClaw rewrite, close, or replace stale issues `#5-#19` with the proposed Phase 1 queue?
5. May OpenClaw inspect and prune stale branches after confirming they are merged or obsolete?
6. Is the `Viewer` role required in Phase 1, or should Phase 1 only include Owner and Contributor?
7. Should Phase 1 persistence target Cosmos DB immediately, or use repository abstractions with a staged durable persistence rollout?
8. Should deploy hardening happen before OAuth/workspace implementation, or run as a parallel infrastructure track?
