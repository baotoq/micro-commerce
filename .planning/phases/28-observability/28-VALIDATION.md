---
phase: 28
slug: observability
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-02
---

# Phase 28 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Manual validation via kubectl + curl (infrastructure-only phase) |
| **Config file** | N/A — no application code changes |
| **Quick run command** | `kubectl get pods -n micro-commerce -l app=otel-collector && kubectl get pods -n micro-commerce -l app=aspire-dashboard` |
| **Full suite command** | `kubectl logs deployment/otel-collector -n micro-commerce --tail=20 && curl -s -o /dev/null -w "%{http_code}" http://localhost:38888` |
| **Estimated runtime** | ~5 seconds |

---

## Sampling Rate

- **After every task commit:** Run `kubectl get pods -n micro-commerce` (verify pod status)
- **After every plan wave:** Run full suite command
- **Before `/gsd:verify-work`:** Full suite must be green + manual Dashboard trace verification
- **Max feedback latency:** 5 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| TBD | 01 | 1 | OBSV-01 | smoke | `kubectl logs deployment/otel-collector -n micro-commerce \| grep -i "exporting"` | N/A | ⬜ pending |
| TBD | 01 | 1 | OBSV-01 | smoke | `kubectl get deployment apiservice -n micro-commerce -o yaml \| grep OTEL_EXPORTER_OTLP_ENDPOINT` | N/A | ⬜ pending |
| TBD | 01 | 1 | OBSV-02 | smoke | `curl -s -o /dev/null -w "%{http_code}" http://localhost:38888` | N/A | ⬜ pending |
| TBD | 01 | 1 | OBSV-02 | manual-only | Open browser at localhost:38888, verify traces visible | N/A | ⬜ pending |
| TBD | 01 | 1 | Success-3 | smoke | `kubectl get pods -n micro-commerce -l app=otel-collector -o jsonpath='{.items[0].status.containerStatuses[0].restartCount}'` | N/A | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

*Existing infrastructure covers all phase requirements.*

This phase is purely Kubernetes manifest work. No test framework setup needed. Validation is smoke tests via kubectl and curl against a running kind cluster.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Dashboard shows live traces from checkout flow | OBSV-02 | Requires visual verification of distributed trace spanning ApiService, saga, and RabbitMQ consumers | 1. Open http://localhost:38888 in browser 2. Trigger a checkout flow via storefront 3. Navigate to Traces tab 4. Verify end-to-end trace visible with all services |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 5s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
