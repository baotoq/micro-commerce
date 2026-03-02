---
phase: 27
slug: cicd-gitops-loop-closure
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-02
---

# Phase 27 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | GitHub Actions workflow validation + kubectl assertions |
| **Config file** | `.github/workflows/container-images.yml` |
| **Quick run command** | `act -j update-manifests --dryrun` or manual workflow dispatch |
| **Full suite command** | Push to master and verify end-to-end GitOps loop |
| **Estimated runtime** | ~300 seconds (5 min SLA target) |

---

## Sampling Rate

- **After every task commit:** Validate YAML syntax with `yq` or `kustomize build`
- **After every plan wave:** Run `kustomize build` on overlays to verify image tag substitution
- **Before `/gsd:verify-work`:** Full push-to-deploy loop must complete
- **Max feedback latency:** 300 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 27-01-01 | 01 | 1 | CICD-02 | integration | `kustomize build deploy/k8s/overlays/production` | ✅ | ⬜ pending |
| 27-01-02 | 01 | 1 | CICD-02 | integration | `gh workflow view container-images.yml` | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

*Existing infrastructure covers all phase requirements — Kustomize overlays, ArgoCD, and GitHub Actions workflow already in place from Phases 23-26.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| End-to-end GitOps loop | CICD-02 | Requires actual push to master + ArgoCD cluster | Push a commit, verify pod image tag matches commit SHA within 5 min |
| Deployment traceability | CICD-02 | Requires checking running pod image against Git commit | `kubectl get pods -o jsonpath='{.items[*].spec.containers[*].image}'` and compare to commit SHA |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 300s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
