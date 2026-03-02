---
phase: 26
slug: argocd-gitops
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-02
---

# Phase 26 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Shell scripts + kubectl assertions |
| **Config file** | none — infrastructure validation via kubectl |
| **Quick run command** | `kubectl get applications -n argocd -o json \| jq '.items[] \| {name: .metadata.name, health: .status.health.status, sync: .status.sync.status}'` |
| **Full suite command** | `bash infra/k8s/scripts/validate-argocd.sh` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run quick kubectl check
- **After every plan wave:** Run full validation script
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 26-01-01 | 01 | 1 | GOPS-01 | infra | `kubectl get ns argocd` | ❌ W0 | ⬜ pending |
| 26-01-02 | 01 | 1 | GOPS-01 | infra | `kubectl get applications -n argocd` | ❌ W0 | ⬜ pending |
| 26-02-01 | 02 | 1 | GOPS-02 | infra | `kubectl get applications -n argocd -o json` | ❌ W0 | ⬜ pending |
| 26-02-02 | 02 | 1 | GOPS-02 | manual | Delete deployment, observe self-heal | N/A | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `infra/k8s/scripts/validate-argocd.sh` — validation script for ArgoCD health
- [ ] ArgoCD installed and accessible in kind cluster

*If none: "Existing infrastructure covers all phase requirements."*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Self-heal on manual deletion | GOPS-02 | Requires manual kubectl delete + observation | 1. `kubectl delete deploy <name> -n dev` 2. Wait ~5s 3. `kubectl get deploy -n dev` — should be restored |
| ArgoCD UI accessible | GOPS-01 | Browser access to UI | Navigate to `https://localhost:38443` and verify login |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
