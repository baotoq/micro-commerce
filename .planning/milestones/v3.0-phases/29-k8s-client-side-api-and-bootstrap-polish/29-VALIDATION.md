---
phase: 29
slug: k8s-client-side-api-and-bootstrap-polish
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-02
---

# Phase 29 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | dotnet test (xUnit) + Playwright |
| **Config file** | `src/MicroCommerce.ApiService.Tests/MicroCommerce.ApiService.Tests.csproj` |
| **Quick run command** | `dotnet test src/MicroCommerce.ApiService.Tests --filter "Category!=Integration"` |
| **Full suite command** | `dotnet test src/MicroCommerce.ApiService.Tests` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test src/MicroCommerce.ApiService.Tests --filter "Category!=Integration"`
- **After every plan wave:** Run `dotnet test src/MicroCommerce.ApiService.Tests`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 29-01-01 | 01 | 1 | K8S-API-01 | manual | Browser test in K8s cluster | N/A | ⬜ pending |
| 29-01-02 | 01 | 1 | K8S-API-01 | unit | `dotnet test` | ❌ W0 | ⬜ pending |
| 29-02-01 | 02 | 1 | K8S-BOOT-01 | manual | Run `bootstrap.sh` in kind cluster | N/A | ⬜ pending |
| 29-03-01 | 03 | 1 | K8S-DOCS-01 | manual | Review REQUIREMENTS.md checkboxes | N/A | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

*Existing infrastructure covers all phase requirements.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Client-side API calls reach Gateway in K8s | K8S-API-01 | Requires running K8s cluster with port-forwarding | Deploy to kind, open storefront, verify network tab shows Gateway URL |
| bootstrap.sh waits for observability pods | K8S-BOOT-01 | Requires kind cluster lifecycle | Run `bootstrap.sh`, verify it waits for otel-collector and aspire-dashboard |
| REQUIREMENTS.md traceability table | K8S-DOCS-01 | Documentation review | Inspect REQUIREMENTS.md for OBSV-01/OBSV-02 checkboxes and UI-* entries |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
