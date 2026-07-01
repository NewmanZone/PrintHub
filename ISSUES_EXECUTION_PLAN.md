# Issue Execution Plan

## Updated By OpenClaw PM

Generated: 2026-06-30T16:49:54Z  
Repo: NewmanZone/PrintHub  
Planning document: ISSUES_EXECUTION_PLAN.md  
PM mode: autonomous  
PR completion: owner approval

## Goal

Phase 1 is the shared Etsy file-preparation workspace:

1. Sign in with OAuth.
2. Create or select a workspace.
3. Connect one Etsy shop.
4. Invite a contributor.
5. Import or create products.
6. Attach versioned STL/3MF files to products and parts.
7. Prepare each Etsy order into the correct downloadable file bundle and manifest.
8. Download the bundle, print manually, and mark the bundle/order complete.

Bambu, OctoEverywhere, direct print submission, live printer status, automatic printer queues, inventory forecasting, billing, and multi-shop subscriptions are later-phase work. They must stay out of the active Phase 1 plan unless the owner explicitly re-scopes the product.

## Current Implementation Status

PrintHub is not Phase 1 complete.

The active implementation work is PR #30, `feat/issue-9-queue-planning`, titled "Draft: Package validated PrintHub backend and queue work." It is draft, mergeable, and latest GitHub CI passed on head SHA `56d8c7d2ea479a084889d5b359a1ef012f83d9ea`.

PR #30 is not promotable yet. The baseline found structural blockers that weaken confidence in the passing CI result:

- `src/PrintHub.API/Controllers/ShopsController.cs` appears under a mis-cased API root while the expected compiled API root is `src/PrintHub.Api`.
- `src/PrintHub.Tests/Unit/ShopServiceTests.cs`, `src/PrintHub.Tests/Unit/ShopsControllerTests.cs`, and `src/PrintHub.Tests/Unit/TokenEncryptionServiceTests.cs` appear outside the expected compiled test root, `tests/PrintHub.Tests`.
- The latest CI success may not prove those changed files are compiled or tested.
- The workspace is dirty only because of untracked `pr-body.txt`; do not commit it accidentally.
- Branch `feat/issue-9-queue-planning` is synced with upstream, 5 commits ahead of `main`, and 0 behind.
- Branches `fix/issue-2`, `fix/issue-3`, and `fix/issue-14` need classification before autonomous agents treat them as active work.

Normal feature work should pause until the PR #30 path/project inclusion blockers are fixed or explicitly waived by the owner.

## Status Versus Documentation

The repo documentation now consistently defines Phase 1 as manual Etsy file preparation, not printer execution.

Aligned with documentation:

- README, architecture, data model, security, and Phase 1 docs describe a shared Etsy workspace.
- OAuth-only authentication is a design lock.
- Phase 1 includes workspace-scoped authorization, Etsy connection, contributor access, product/file versioning, order preparation, downloadable bundles, and manual completion.
- Source STL/3MF files are retained by default with user-controlled deletion/purge.
- Printer adapters, live printer status, and direct print submission are later-phase.

Not aligned:

- GitHub issues #5-#19 still reflect older broader work, including printer adapters, inventory, queue planning, and broad frontend pages.
- This planning document proposes Phase 1 issues #101-#113, but those issues do not exist in GitHub yet.
- `DESIGN/dotnet-structure.md` says `src/PrintHub.API`, while branch health expects `src/PrintHub.Api`.
- Duplicate/stale roots exist in local state: `src/PrintHub.API` and `src/PrintHub.Tests` alongside expected roots `src/PrintHub.Api` and `tests/PrintHub.Tests`.
- `DESIGN/print-queue.md` is marked later-phase, but its filename and content can still mislead agents.
- README contains a blank architecture diagram block.

## Divergences From Documentation

- `Intended requirement`: Phase 1 excludes Bambu, OctoEverywhere, direct print submission, live printer telemetry, and automatic printer queues.
  - Evidence: README, architecture, data model, Phase 1, printer integration, and Bambu spike docs.
  - Action: Keep printer execution work in the later-phase bucket.

- `Intended requirement`: Authentication is OAuth-only.
  - Evidence: README design lock, API design, data model, and security docs.
  - Action: Do not implement password registration, password login, password reset, or password hash storage.

- `Intended requirement`: Source STL/3MF files are retained by default with user-controlled deletion/purge.
  - Evidence: README, architecture, data model, and security docs.
  - Action: File work must include upload, versioning, download, retention, deletion, and purge semantics.

- `Unintended divergence`: PR #30 uses `src/PrintHub.API` while expected active API root is `src/PrintHub.Api`.
  - Risk: Changed controller code may not compile into the API.
  - Action: Normalize casing/root before PR promotion.

- `Unintended divergence`: PR #30 uses `src/PrintHub.Tests` while expected active test root is `tests/PrintHub.Tests`.
  - Risk: Changed tests may not run even when CI is green.
  - Action: Move/merge tests into the compiled test project and prove execution.

- `Unintended divergence`: `DESIGN/dotnet-structure.md` does not match the detected solution layout.
  - Risk: Future agents may create files in the wrong roots.
  - Action: Update the doc after confirming the canonical structure.

- `Unintended divergence`: GitHub issues are stale against the current Phase 1 plan.
  - Risk: PM cycles may pick later-phase or obsolete work.
  - Action: Realign issues after owner approves the strategy.

- `Needs owner decision`: Whether PR #30 should remain one backend packaging PR or be split/reworked.
  - Action: Keep draft until structural cleanup and test proof are complete.

- `Needs owner decision`: Whether to create new issues #101-#113, rewrite old issues #5-#19, or close old issues as superseded.
  - Action: Prepare issue mapping, then apply only after approval.

- `Needs owner decision`: Whether stale branches `fix/issue-2`, `fix/issue-3`, and `fix/issue-14` should be deleted, archived, inspected, or left alone.
  - Action: Classify before cleanup.

## Gaps To Phase 1 End Goal

- [ ] Confirm canonical .NET project root casing and test layout.
- [ ] Ensure PR #30 code and tests are included in compiled projects.
- [ ] Implement or verify workspace-scoped Phase 1 data model.
- [ ] Implement or verify OAuth-only current-user bootstrap.
- [ ] Implement or verify workspace creation and selection.
- [ ] Implement or verify contributor invite and workspace membership flow.
- [ ] Implement or verify Etsy shop OAuth connection.
- [ ] Implement or verify Etsy listing sync.
- [ ] Implement or verify Etsy order sync/manual refresh.
- [ ] Implement or verify product library and Etsy listing linkage.
- [ ] Implement or verify product-to-part mapping.
- [ ] Implement or verify STL/3MF upload, versioning, current-version selection, download, deletion, and purge.
- [ ] Implement or verify preparation bundle and manifest generation.
- [ ] Implement or verify personalization capture and manual fallback.
- [ ] Implement or verify manual bundle/order completion.
- [ ] Implement or verify Phase 1 frontend app shell.
- [ ] Implement or verify workspace settings UI for Etsy and contributors.
- [ ] Implement or verify product/file management UI.
- [ ] Implement or verify order preparation/download UI.
- [ ] Add end-to-end tests and responsive/accessibility verification for the full Phase 1 path.

## Blocking Work Before Normal Feature Development

### P0: Resolve PR #30 Structural Path Blockers

- [ ] Inspect `PrintHub.sln` and project files to confirm the actual compiled API root.
- [ ] Inspect `PrintHub.sln` and project files to confirm the actual compiled test root.
- [ ] Move or merge `src/PrintHub.API/Controllers/ShopsController.cs` into the compiled API project root, expected `src/PrintHub.Api`.
- [ ] Move or merge `src/PrintHub.Tests/Unit/ShopServiceTests.cs` into the compiled test project root, expected `tests/PrintHub.Tests`.
- [ ] Move or merge `src/PrintHub.Tests/Unit/ShopsControllerTests.cs` into the compiled test project root, expected `tests/PrintHub.Tests`.
- [ ] Move or merge `src/PrintHub.Tests/Unit/TokenEncryptionServiceTests.cs` into the compiled test project root, expected `tests/PrintHub.Tests`.
- [ ] Remove or retire duplicate/mis-cased project folders if they are unused.
- [ ] Confirm no changed source/test files live outside compiled project roots.
- [ ] Keep PR #30 draft until this is complete.

Acceptance criteria:

- `PrintHub.sln` includes the intended API and test projects.
- `ShopsController` is compiled by the API project.
- `ShopServiceTests`, `ShopsControllerTests`, and `TokenEncryptionServiceTests` are compiled and executed.
- No active changed files remain under duplicate/mis-cased project roots.
- `pr-body.txt` is not committed unless intentionally needed.

Verification proof:

- `dotnet test PrintHub.sln`
- Project inclusion evidence for the moved controller and tests.
- Case-sensitive path audit showing no active `PrintHub.API` versus `PrintHub.Api` mismatch.
- Git status showing only intentional dirty files.
- GitHub CI success on the final pushed head SHA.

### P0: Decide PR #30 Promotion Strategy

- [ ] Review corrected PR #30 against the Phase 1 scope.
- [ ] Decide whether it remains one backend foundation PR.
- [ ] If too broad or mis-scoped, split/rework it.
- [ ] If valid, update the PR body with scope, non-goals, and test proof.
- [ ] Move from draft only after owner approval.

Acceptance criteria:

- PR title/body accurately describe the shipped work.
- The PR does not imply unverified printer/queue execution behavior.
- Structural blockers are cleared.
- Final CI is green.
- Owner approval is recorded before ready-for-review or merge.

Verification proof:

- PR diff contains no misplaced files.
- PR body includes tests/checks run.
- GitHub CI is successful on final head SHA.

## Prioritized Implementation Plan

### P1: Align Docs And GitHub Issues

- [ ] Update `DESIGN/dotnet-structure.md` to match the confirmed solution layout and casing.
- [ ] Clarify, rename, or archive `DESIGN/print-queue.md` as Phase 3 reference so agents do not treat it as active Phase 1 scope.
- [ ] Fill or remove the blank README architecture block.
- [ ] Prepare owner-approved issue realignment for #5-#19.
- [ ] Apply owner-approved strategy: create #101-#113, rewrite existing issues, or close/supersede stale issues.
- [ ] Label printer, inventory, billing, and queue execution work as later-phase.
- [ ] Classify branches `fix/issue-2`, `fix/issue-3`, and `fix/issue-14`.

Acceptance criteria:

- Docs reflect the actual project structure.
- Active GitHub issues match the Phase 1 sequence.
- Later-phase work is clearly out of scope.
- Stale branches have an owner-approved disposition.

Verification proof:

- Documentation diff reviewed against solution layout.
- GitHub issue list shows aligned Phase 1 work items or explicit superseded/later-phase labels.
- Branch classification is recorded.

### P2: Backend Phase 1 Data Model And Workspace Authorization

Proposed issue: 101  
Can start after: P0

- [ ] Implement/verify User, Workspace, WorkspaceMember, Shop, Product, Part, ProductPart, PrintFile, PrintFileVersion, EtsyOrder, EtsyOrderItem, PreparationBundle, PreparationBundleItem, and AuditEvent entities as needed for Phase 1.
- [ ] Implement/verify workspace-scoped authorization.
- [ ] Implement/verify repository boundaries for Phase 1 entities.
- [ ] Enforce workspace membership on workspace-owned reads/writes.
- [ ] Add tests for owner, contributor, viewer if retained, and non-member denial.

Acceptance criteria:

- Workspace membership gates every workspace-scoped operation.
- No password-auth fields or flows are introduced.
- Model supports one Etsy shop per workspace for Phase 1.
- Authorization tests cover allowed and denied paths.

Verification proof:

- `dotnet test PrintHub.sln`
- Unit tests for authorization and repository behavior.
- API/controller tests where endpoints exist.

### P2: OAuth-Only Auth And Current User Bootstrap

Proposed issue: 102  
Can start after: P0  
Can run in parallel with: 101, if identity contracts are agreed

- [ ] Implement/verify JWT bearer validation.
- [ ] Implement/verify `GET /auth/me`.
- [ ] Bootstrap user profile on first valid OAuth sign-in.
- [ ] Return user profile and workspace memberships.
- [ ] Implement/verify API-side logout only if API-side session state exists.
- [ ] Add tests proving password endpoints and password hash storage are absent.

Acceptance criteria:

- Valid OAuth JWTs are accepted.
- Invalid/missing tokens are rejected.
- `GET /auth/me` returns user and workspace membership data.
- First sign-in creates a profile idempotently.
- No password registration/login/reset exists.

Verification proof:

- `dotnet test PrintHub.sln`
- Auth tests for valid, invalid, and missing tokens.
- Contract test for `/auth/me`.

### P2: Frontend App Shell Aligned To Phase 1

Proposed issue: 106  
Can start after: P0  
Can use mocks until APIs are ready

- [ ] Align navigation to Phase 1: workspace, Etsy connection, contributors, products/files, orders/preparation bundles.
- [ ] Keep printer pages, jobs pages, queue execution, inventory, and billing out of active navigation.
- [ ] Preserve the light operations theme.
- [ ] Add loading, empty, error, and permission-denied states.
- [ ] Verify responsive layout.

Acceptance criteria:

- UI presents the usable Phase 1 workflow.
- Later-phase functionality is not shown as active.
- No horizontal overflow at target widths.
- Primary controls are keyboard reachable.

Verification proof:

- Frontend install/build/test commands from `frontend/package.json`.
- Manual checks at about 1440px, 768px, and 390px.
- Keyboard navigation notes.

### P3: Workspace Members And Contributor Invites

Proposed issue: 103  
Wait for: 101, 102  
Blocks: 110

- [ ] Implement member listing.
- [ ] Implement contributor invite creation.
- [ ] Implement invite acceptance or document a temporary manual/admin flow if email delivery is deferred.
- [ ] Enforce owner-only invite management.
- [ ] Add audit events for membership changes.

Acceptance criteria:

- Owners can invite contributors.
- Contributors can access allowed workspace areas after acceptance.
- Non-owners cannot manage invites.
- Removed members lose access.

Verification proof:

- `dotnet test PrintHub.sln`
- API tests for invite happy path and denied paths.
- Manual API verification notes.

### P3: Etsy Connection, Listing Sync, And Order Sync

Proposed issue: 104  
Wait for: 101, 102  
Blocks: 107, 108

- [ ] Implement/verify one Etsy shop connection per workspace.
- [ ] Encrypt and store Etsy tokens.
- [ ] Support token refresh/revocation handling.
- [ ] Import/sync Etsy listings.
- [ ] Import/sync Etsy orders or provide manual refresh.
- [ ] Retain order context required for preparation bundles and personalization.

Acceptance criteria:

- Workspace owner can connect one Etsy shop.
- Tokens are encrypted at rest and never logged.
- Listing sync creates or updates linked product candidates.
- Order sync stores order and item context.
- Failures do not leak tokens.

Verification proof:

- `dotnet test PrintHub.sln`
- Token encryption tests.
- Etsy service/controller tests.
- Manual verification with safe fake Etsy configuration.

### P3: File Storage, Upload, Versioning, Download, And Purge

Proposed issue: 105  
Wait for: 101, 102  
Blocks: 107, 108

- [ ] Allow STL and 3MF uploads.
- [ ] Enforce file type allowlist and size limits.
- [ ] Store source files privately.
- [ ] Create file version records.
- [ ] Support current-version selection.
- [ ] Support signed/private downloads.
- [ ] Support deletion and purge controls.
- [ ] Test unauthorized access and invalid files.

Acceptance criteria:

- Files are not public.
- Users can upload, version, select current, download, delete, and purge within permissions.
- Invalid file types are rejected.
- Retention behavior matches documentation.

Verification proof:

- `dotnet test PrintHub.sln`
- File service tests.
- API tests for upload/download/delete/purge.
- Manual verification with sample STL/3MF files.

### P4: Product, Part, And File Mapping API

Proposed issue: 107  
Wait for: 101, 104, 105  
Blocks: 108, 111

- [ ] Implement product CRUD for imported and manual products.
- [ ] Link products to Etsy listing IDs.
- [ ] Implement part CRUD.
- [ ] Map products to parts.
- [ ] Map parts to current file versions.
- [ ] Support personalization metadata and manual customization flags.

Acceptance criteria:

- Products can be imported or created manually.
- Products map to printable parts.
- Parts reference current file versions.
- Personalized products retain Etsy personalization context.

Verification proof:

- `dotnet test PrintHub.sln`
- API tests for product/part/file mappings.
- Tests for quantity and personalization metadata.

### P4: Order Preparation Bundle API And Manifest Generation

Proposed issue: 108  
Wait for: 104, 105, 107  
Blocks: 112

- [ ] Generate preparation bundles from Etsy orders.
- [ ] Resolve products, parts, current file versions, quantities, and personalization.
- [ ] Generate a manifest with file names, counts, order context, and manual steps.
- [ ] Support manual download.
- [ ] Support marking bundle/order preparation complete.
- [ ] Surface unresolved mappings as actionable errors.

Acceptance criteria:

- A valid mapped order produces a downloadable bundle and manifest.
- Quantity greater than one is represented correctly.
- Personalized orders show personalization data clearly.
- Missing mappings block completion with clear errors.
- Manual completion updates status.

Verification proof:

- `dotnet test PrintHub.sln`
- Bundle service tests for standard, quantity, personalized, and missing-mapping cases.
- API tests for prepare, download, and complete actions.

### P4: Public Entry And OAuth UI

Proposed issue: 109  
Wait for: 106

- [ ] Provide a concise public entry page if needed.
- [ ] Make OAuth sign-in the primary action.
- [ ] Route signed-in users to workspace selection or dashboard.
- [ ] Avoid presenting later-phase features as active.

Acceptance criteria:

- Signed-out users can start OAuth sign-in.
- Signed-in users land in the app workflow.
- Copy matches Phase 1 scope.

Verification proof:

- Frontend build/test commands.
- Manual browser verification for signed-out and signed-in states.

### P4: Workspace Settings UI For Etsy And Contributors

Proposed issue: 110  
Wait for: 103, 104, 106

- [ ] Show connected Etsy shop state.
- [ ] Provide connect, refresh, disconnect/revoke states as backend supports them.
- [ ] Show members and pending invites.
- [ ] Allow owner invite actions.
- [ ] Show permission-denied state for non-owners.

Acceptance criteria:

- Owners can manage Etsy connection and invites.
- Contributors see allowed settings without owner controls.
- Loading, empty, error, and permission-denied states exist.

Verification proof:

- Frontend tests where available.
- Manual responsive checks at 1440px, 768px, and 390px.
- Keyboard navigation check.

### P4: Product And Part File-Management UI

Proposed issue: 111  
Wait for: 106, 107  
Blocks: 112

- [ ] List products.
- [ ] Show product detail and Etsy linkage.
- [ ] Manage parts and mappings.
- [ ] Upload STL/3MF files.
- [ ] Show file versions and current-version selection.
- [ ] Support download, deletion, and purge controls.
- [ ] Show personalization/manual customization fields.

Acceptance criteria:

- User can create or inspect a product, attach parts, upload files, select current versions, and download files.
- UI prevents invalid file uploads.
- Permission and empty states are clear.

Verification proof:

- Frontend build/test commands.
- Manual UI verification with sample STL/3MF files.
- Responsive and keyboard checks.

### P5: Orders And Preparation Bundle UI

Proposed issue: 112  
Wait for: 106, 108, 111  
Blocks: 113

- [ ] List synced Etsy orders.
- [ ] Show order detail, items, quantities, and personalization.
- [ ] Show mapping readiness per item.
- [ ] Generate preparation bundle.
- [ ] Download bundle and manifest.
- [ ] Mark bundle/order manually complete.
- [ ] Show actionable errors for missing mappings/files.

Acceptance criteria:

- User can move an order from synced to prepared to downloaded/manual complete.
- Missing mappings link back to product/file management.
- Personalized order data is visible.
- UI remains manual-download focused.

Verification proof:

- Frontend build/test commands.
- Manual end-to-end UI verification with safe fake order data.
- Responsive and keyboard checks.

### P5: Phase 1 End-To-End Quality Gate

Proposed issue: 113  
Wait for: 109, 110, 111, 112

- [ ] Add end-to-end coverage for OAuth entry or mocked auth.
- [ ] Cover workspace selection, Etsy connection state, product/file setup, order preparation, bundle download, and manual completion.
- [ ] Verify desktop, tablet, and mobile layouts.
- [ ] Verify keyboard navigation.
- [ ] Verify loading, empty, error, and permission-denied states.
- [ ] Verify later-phase features are not exposed as active workflows.
- [ ] Update README with final Phase 1 run/test instructions.

Acceptance criteria:

- Phase 1 happy path is covered by automated or documented manual tests.
- Responsive and accessibility checks pass.
- README explains how to run and verify the app.
- Owner can demo the Phase 1 workflow end to end.

Verification proof:

- `dotnet test PrintHub.sln`
- Frontend install/build/test/e2e commands from `frontend/package.json`
- Manual verification notes and screenshots where useful.
- Final GitHub CI success.

## Parallel Groups

```yaml
groups:
  p0_blockers:
    - resolve_pr_30_structural_paths
    - decide_pr_30_promotion_strategy

  wave_0_after_p0:
    - 101 # backend data model/workspace authorization
    - 102 # OAuth profile bootstrap
    - 106 # frontend shell/navigation
    - docs_and_issue_alignment

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

- PR #30 structural path cleanup must happen before normal feature work or PR promotion.
- Product/part/file mapping API waits for data model, Etsy sync contracts, and file storage.
- Order bundle generation waits for product mappings and file versioning.
- Orders UI waits for bundle API and product/file UI contracts.
- End-to-end testing waits for user-facing Phase 1 flows to exist.

## Can Run In Parallel

- OAuth bootstrap and workspace data model can run together if user identity contracts are agreed.
- Frontend shell/navigation can run with mocks while backend foundations are built.
- Etsy sync and file storage can run in parallel after workspace authorization exists.
- Contributor settings UI can run in parallel with product/file UI after the app shell exists.
- Docs and issue alignment can run alongside implementation after P0 structure is known.

## Immediate Next PM Focus For The Next 2-Hour Cycle

Primary focus: unblock PR #30 by proving or fixing project-root inclusion.

- [ ] Inspect `PrintHub.sln` and project files to confirm canonical API and test roots.
- [ ] Move/merge misplaced PR #30 files from `src/PrintHub.API` and `src/PrintHub.Tests` into compiled roots if confirmed.
- [ ] Run `dotnet test PrintHub.sln`.
- [ ] Produce proof that `ShopsController`, `ShopServiceTests`, `ShopsControllerTests`, and `TokenEncryptionServiceTests` are compiled/executed.
- [ ] Run a case-sensitive path audit for `PrintHub.API` versus `PrintHub.Api`.
- [ ] Check `git status` and keep `pr-body.txt` uncommitted unless intentionally needed.
- [ ] Push corrected branch only if tests pass.
- [ ] Keep PR #30 draft and report whether it is ready for owner promotion review or needs splitting.

Do not start printer, inventory, queue, billing, or broad frontend feature work during this cycle.

## Required Checks Before PR Promotion Or Merge

- [ ] `dotnet test PrintHub.sln`
- [ ] `pytest`, if Python tooling remains applicable
- [ ] Project inclusion check proving changed files are compiled
- [ ] Case-sensitive path audit for `PrintHub.API` versus `PrintHub.Api`
- [ ] Source/test path audit confirming no changed files live outside compiled roots
- [ ] Git status check confirming only intentional files are dirty
- [ ] GitHub CI success on final pushed head SHA
- [ ] For frontend changes: install/build/test/e2e checks from `frontend/package.json`

## Manual Verification Gate

Every implementation PR should include manual verification notes. For UI PRs, verify at minimum:

- Desktop width around 1440px.
- Tablet width around 768px.
- Mobile width around 390px.
- No horizontal overflow.
- Keyboard navigation reaches every primary control.
- Empty, loading, error, and permission-denied states are visible and styled.
- Light theme remains the default.
- Later-phase printer/inventory/billing features are not presented as active Phase 1 workflows.

## Later-Phase Issue Bucket

Keep these out of Phase 1 unless the owner explicitly re-scopes:

- Bambu/OctoEverywhere printer adapters.
- Direct cloud print submission.
- Live printer telemetry.
- Automatic printer queues.
- Automatic slicing or build-plate packing.
- Inventory movements, cost records, low-stock alerts, and reorder intelligence.
- Billing and multi-shop subscriptions.
- Production Azure hardening beyond what is needed to safely test Phase 1.

## GitHub Issue Realignment Recommendation

Recommended owner-approved target state:

- Keep or rewrite #5 as Phase 1 data model/workspace authorization if it maps cleanly.
- Keep or rewrite #6 as OAuth-only auth/current-user bootstrap.
- Keep or rewrite #7 as Etsy connection/listing/order sync.
- Close or relabel #10 as later-phase printer integration.
- Rewrite #11 only if a public entry/OAuth UI remains desired.
- Rewrite #12 away from queue planning and toward Phase 1 app shell/dashboard.
- Rewrite #13 as product/part/file management UI.
- Rewrite #14 to remove printer/jobs scope and keep only settings/orders pieces that match Phase 1, or close as superseded.
- Close or relabel #16 as later-phase inventory.
- Rewrite #17 to remove queue handoff and focus on Etsy order sync/preparation bundle handoff, or close as superseded by #104/#108.
- Keep #18 only if rewritten as real API clients/OAuth/authenticated data loading for Phase 1.
- Keep #19 only if deployment docs are required for Phase 1; otherwise relabel as later-phase/ops.

Alternative target state:

- Create new issues #101-#113 from this plan.
- Close old issues #5-#19 as superseded or relabel them as later-phase references.
- Use this document as the issue body source of truth.

Do not mutate GitHub issue state until owner approves one strategy.

## Open Questions For Owner

- Should PR #30 remain the active backend packaging PR after structural cleanup, or should it be split into smaller Phase 1 foundation PRs?
- Should GitHub issues be realigned by rewriting #5-#19 or by creating new #101-#113 issues and closing/superseding the old set?
- Should stale branches `fix/issue-2`, `fix/issue-3`, and `fix/issue-14` be deleted, archived, inspected, or left alone?
- Is `Viewer` a real Phase 1 workspace role, or should Phase 1 ship only `Owner` and `Contributor`?
- Should invite acceptance require real email delivery in Phase 1, or is a manual/admin acceptance flow acceptable for the first usable version?
- Is `pytest` still an expected repo check, or is it legacy tooling that should be removed from PM verification gates?
- Should Phase 1 include Azure deployment work, or is local/CI-verified functionality enough before deployment hardening?
- Should README’s architecture block be filled now with the current Azure architecture or removed until implementation stabilizes?
