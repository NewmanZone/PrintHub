# Bambu Integration Spike - PrintHub

## Status

Completed on 2026-05-10. This is historical Phase 3 reference material, not a Phase 1 requirement.

## Current Product Decision

Bambu integration is plausible, but PrintHub Phase 1 does not require it. The Phase 1 product goal is shared Etsy file preparation with manual bundle downloads.

Use this spike only when planning the later printer execution phase.

## Findings Summary

- Bambu cloud integration appears technically viable.
- The Bambu ecosystem provides cloud-based printer discovery/status concepts.
- Community documentation suggests possible routes for status and submission workflows.
- API stability, auth handling, and support expectations still need careful product review before production use.

## Later-Phase Recommendation

When Phase 3 starts:

1. Define a printer adapter contract from the Phase 1 preparation bundle model.
2. Re-validate the latest official Bambu Connect/developer terms.
3. Prefer documented/approved APIs where possible.
4. Keep Bambu credentials and tokens encrypted.
5. Make printer submission optional; manual downloads must continue to work.

## Not Phase 1

- User-entered Bambu credentials.
- Printer discovery.
- Live printer status.
- Direct print submission.
- MQTT progress streaming.
- LAN-only Bambu support.
