# Issue Execution Plan

## Goal
Track the implementation order for PrintHub MVP and make it clear which issues can run in parallel versus which must wait for prior PRs to merge.

## Dependencies

```yaml
dependencies:
  5: [2]                    # Data layer depends on backend scaffold
  6: [2]                    # Auth depends on backend scaffold
  7: [5, 6]                 # Etsy sync depends on data + auth
  8: [5]                    # Catalog depends on data layer
  9: [8, 5]                 # Queue planner depends on catalog + data
  10: [9]                   # Printer adapter depends on queue planner
  11: [3]                   # Landing page depends on frontend scaffold
  12: [3]                   # Dashboard depends on frontend scaffold
  13: [3]                   # Catalog pages depend on frontend scaffold
  14: [3]                   # Printers/settings depend on frontend scaffold
  15: [2, 3]               # QA smoke tests depend on both scaffolds
  16: [8, 5]               # Inventory depends on catalog + data
  17: [6, 7, 8, 9]         # Personalized orders depends on auth + etsy + catalog + queue
  18: [6, 8, 9, 10, 16, 17]  # Frontend integration depends on all backend domains
  19: [2]                   # Infra depends on backend scaffold
```

## Parallel Groups

```yaml
groups:
  foundation: [1, 2, 3, 4]           # Wave 0: start immediately in parallel
  backend_data_auth: [5, 6]         # Wave 1: after backend scaffold merges
  frontend_mock_pages: [11, 12, 13, 14]  # Wave 1: after frontend scaffold merges
  qa_infra: [15, 19]                # Wave 1: after both scaffolds
  backend_domain: [7, 8, 9, 16]     # Wave 2: after data/auth ready
  printer_orders: [10, 17]          # Wave 3: after catalog/queue ready
  integration: [18]                 # Wave 4: after all backend domains
```

## External PRs

```yaml
external_prs:
  {}   # No external PRs blocking this execution plan
```

## Constraints

```yaml
max_parallel: 6
review_after_each: false
auto_merge: false
```

## Notes

- Issue #1 (design lock) is source-of-truth work. Implementation PRs must not contradict its decisions.
- Issue #4 (Bambu spike) is independent and gates printer adapter scope (#10).
- Issue #15 (QA) starts small after scaffolds and expands as routes stabilize.
- Issue #19 (infra) starts with skeleton after backend scaffold and validates after deployment.
- Wave ordering ensures backend API contracts exist before frontend integration (#18) begins.
- Dependencies within a group are still respected (e.g., #9 waits for #8 even though both are in backend_domain group).
