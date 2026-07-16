# Captain's Chair Usage Policy

This repository uses provider-reported tokens, grouped by model and workflow stage. Credits,
synthetic prices, and guessed token counts are not accepted as usage evidence.

## Model Routing

- Use GPT-5.5 for course readiness, independent review, comment adjudication when findings
  conflict, and final exit review.
- Use `gpt-5.3-codex-spark` for coding, deterministic test repair, and UI QA only after the
  configured harness passes a route canary for that exact model identifier.
- Until that canary passes, use the verified direct Codex route. Do not silently substitute a
  different model.
- Local coding models may be introduced only through a recorded, repository-specific canary
  that proves valid diffs, required checks, and acceptable first-pass review quality.

## Admission And Stops

- A scheduled cycle must not start a model when repository evidence has not changed since an
  identical no-progress transition.
- Every attempt records requested model, resolved model, stage, input, cached input, reasoning,
  output, and total tokens when the provider reports them. Failed attempts and fallback reasons
  remain visible.
- Unknown or failed telemetry blocks autonomous dispatch. Supervised work may continue only when
  the owner has initiated the bounded action and schedules remain disabled.
- Autonomous mode requires explicit daily and per-model token limits in private Captain's Chair
  configuration. Those operational values do not belong in this public repository.
- Readiness and review loops are capped at one fresh call after a material course, code, provider,
  or policy evidence change.

## Reporting

Transition summaries report the work package, outcome, proof link, next action, and tokens by
model. Repeated prompt fingerprints, fallback churn, failed-attempt tokens, and stages with the
highest token use are included in diagnostics rather than repeated routine notifications.
