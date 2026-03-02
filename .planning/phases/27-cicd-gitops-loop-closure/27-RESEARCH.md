# Phase 27: CI/CD GitOps Loop Closure - Research

**Researched:** 2026-03-02
**Domain:** GitHub Actions CI/CD, Kustomize image tag automation, ArgoCD GitOps sync
**Confidence:** HIGH

## Summary

Phase 27 closes the GitOps loop: a push to `master` triggers the existing `container-images.yml` workflow which builds and pushes SHA-tagged images to ghcr.io, then a new job in that same workflow updates the Kustomize dev overlay `kustomization.yaml` files with the new SHA-based image tags and commits the change back to the repo. ArgoCD (installed in Phase 26 with automated sync + self-heal) detects the Git change and rolls out updated pods automatically.

The existing infrastructure is well-positioned for this. The `container-images.yml` workflow already builds all three images (apiservice, gateway, web) with `type=sha,prefix=sha-` tags via `docker/metadata-action@v5`, producing tags like `sha-860c190`. The Kustomize dev overlays (`infra/k8s/overlays/dev/{service}/kustomization.yaml`) already have per-service `images:` blocks with `newName` and `newTag` fields. ArgoCD Applications already point to these overlay paths with automated sync enabled. The only missing piece is a GitHub Actions job that updates `newTag` in each overlay's `kustomization.yaml` and commits the change.

A critical design consideration is infinite loop prevention. When the workflow commits updated image tags back to `master`, it must not re-trigger itself. GitHub Actions has built-in protection: events triggered by `GITHUB_TOKEN` do **not** create new workflow runs (except for `workflow_dispatch` and `repository_dispatch`). Since the `container-images.yml` workflow triggers on `push: branches: [master]`, a commit pushed via `GITHUB_TOKEN` will not re-trigger it. As an additional safety measure, the commit message should include `[skip ci]`.

**Primary recommendation:** Add an `update-manifests` job to the existing `container-images.yml` workflow that runs after all three build jobs complete. This job checks out the repo, runs `kustomize edit set image` in each per-service overlay directory to update `newTag` to `sha-<short-sha>`, commits the three modified `kustomization.yaml` files, and pushes to `master` using `GITHUB_TOKEN` with `contents: write` permission. The overlay `newName` fields must also change from local names (`apiservice`, `gateway`, `web`) to the full ghcr.io registry paths.

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| CICD-02 | CI commits updated SHA image tags to Kustomize dev overlay, triggering ArgoCD sync | The `update-manifests` job uses `kustomize edit set image` to write `sha-<short-sha>` tags into each per-service overlay kustomization.yaml, commits with `[skip ci]`, and pushes via GITHUB_TOKEN. ArgoCD auto-sync detects the tag change within its 3-minute polling interval (or immediately with a webhook) and rolls out new pods. Every image tag in Git matches the commit SHA that built it, providing full traceability. |
</phase_requirements>

## Standard Stack

### Core
| Tool | Version | Purpose | Why Standard |
|------|---------|---------|--------------|
| GitHub Actions | N/A (platform) | CI/CD pipeline; build images + commit manifest updates | Already in use for `container-images.yml`; no new tooling needed |
| Kustomize | built-in (kubectl) | `kustomize edit set image` updates overlay `kustomization.yaml` | Already used in overlays; CLI command is the standard automation pattern for image tag updates |
| docker/metadata-action | v5 | Generates SHA-prefixed image tags (`sha-860c190`) | Already in use in all three build jobs; provides consistent tag format |
| ArgoCD | v3.3.2 | Detects Git changes, syncs cluster state automatically | Installed in Phase 26; auto-sync + self-heal already enabled on all Applications |

### Supporting
| Tool | Version | Purpose | When to Use |
|------|---------|---------|-------------|
| actions/checkout@v4 | v4 | Checkout repo in the update-manifests job | Required to access and modify kustomization.yaml files |
| git (CLI) | built-in | Commit and push updated kustomization.yaml files | Standard approach; no third-party action needed for simple commit+push |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `kustomize edit set image` CLI | Direct sed/yq replacement of newTag | `kustomize edit set image` is the official Kustomize CLI command; sed is fragile and breaks on format changes |
| Commit in same workflow | Separate `repository_dispatch` event | Adds complexity; same-workflow job is simpler and runs only when builds succeed |
| `stefanzweifel/git-auto-commit-action` | Manual `git commit && git push` | Third-party action adds a dependency; manual git commands are ~5 lines and fully transparent |
| ArgoCD polling (3 min default) | GitHub webhook to ArgoCD `/api/webhook` | Webhook gives <5s detection but requires ArgoCD to be internet-accessible; kind cluster is local-only. Polling is sufficient for the success criterion (5 min). Can add webhook later for cloud deployments. |
| Short SHA tag (`sha-860c190`) | Full commit SHA tag | Short SHA (7 chars) is the `docker/metadata-action` default with `type=sha,prefix=sha-`; sufficient for uniqueness and traceability. Already used in the existing workflow. |

**Installation:**
```bash
# No new tools to install. Kustomize is built into the GitHub Actions runner via kubectl.
# If standalone kustomize is needed:
# - uses: imranismail/setup-kustomize@v2
# But kubectl built-in kustomize is sufficient.
```

## Architecture Patterns

### Recommended Workflow Structure
```
.github/workflows/container-images.yml
  build-web:         (existing) Build + push web image to ghcr.io
  build-apiservice:  (existing) Build + push apiservice image to ghcr.io
  build-gateway:     (existing) Build + push gateway image to ghcr.io
  update-manifests:  (NEW) Runs after all 3 builds complete
    - Checkout repo
    - kustomize edit set image for each overlay
    - git commit + push [skip ci]
```

### Pattern 1: Update-Manifests Job with `needs` Dependency
**What:** A GitHub Actions job that depends on all three build jobs (`needs: [build-web, build-apiservice, build-gateway]`), updates Kustomize overlays with the new SHA tag, and commits the change.
**When to use:** When all images in a monorepo are built and deployed together.
**Example:**
```yaml
# Source: Standard GitOps pattern verified via official Kustomize docs + GitHub Actions docs
update-manifests:
  name: Update Kustomize Manifests
  needs: [build-web, build-apiservice, build-gateway]
  runs-on: ubuntu-latest
  permissions:
    contents: write
  steps:
    - name: Checkout
      uses: actions/checkout@v4

    - name: Update image tags in dev overlay
      env:
        SHORT_SHA: sha-${{ github.sha }}  # Will be trimmed to 7 chars below
      run: |
        SHORT_SHA="sha-$(echo '${{ github.sha }}' | cut -c1-7)"

        cd infra/k8s/overlays/dev/apiservice
        kustomize edit set image apiservice=ghcr.io/baotoq/micro-commerce/apiservice:$SHORT_SHA

        cd ../gateway
        kustomize edit set image gateway=ghcr.io/baotoq/micro-commerce/gateway:$SHORT_SHA

        cd ../web
        kustomize edit set image web=ghcr.io/baotoq/micro-commerce/web:$SHORT_SHA

    - name: Commit and push
      run: |
        git config user.name "github-actions[bot]"
        git config user.email "github-actions[bot]@users.noreply.github.com"
        git add infra/k8s/overlays/dev/
        git diff --staged --quiet && echo "No changes to commit" && exit 0
        git commit -m "[skip ci] chore: update image tags to sha-$(echo '${{ github.sha }}' | cut -c1-7)"
        git push
```

### Pattern 2: Overlay Image Name Correction
**What:** The current dev overlay kustomization.yaml files use local-only image names (`apiservice:dev`, `gateway:dev`, `web:dev`) that match the bootstrap script's local builds. For CI/CD, the `newName` must point to the ghcr.io registry path.
**When to use:** When the same overlay serves both local development (kind with local images) and CI/CD (ghcr.io images).
**Key insight:** `kustomize edit set image` with the syntax `<oldName>=<newName>:<newTag>` updates both `newName` and `newTag` in one command. Running `kustomize edit set image apiservice=ghcr.io/baotoq/micro-commerce/apiservice:sha-860c190` updates the overlay from:
```yaml
images:
- name: apiservice
  newName: apiservice
  newTag: dev
```
to:
```yaml
images:
- name: apiservice
  newName: ghcr.io/baotoq/micro-commerce/apiservice
  newTag: sha-860c190
```
This means the base deployment's `image: apiservice:dev` gets transformed by Kustomize to `image: ghcr.io/baotoq/micro-commerce/apiservice:sha-860c190`.

### Pattern 3: SHA Tag Format Alignment
**What:** The `docker/metadata-action` `type=sha,prefix=sha-` generates tags like `sha-860c190` (7-character short SHA). The `update-manifests` job must produce the same format.
**When to use:** Always -- the tag in `kustomization.yaml` must exactly match a tag that exists in ghcr.io.
**Example:**
```bash
# docker/metadata-action produces: sha-860c190 (7 chars)
# To match in shell:
SHORT_SHA="sha-$(echo '${{ github.sha }}' | cut -c1-7)"
# Result: sha-860c190
```

### Anti-Patterns to Avoid
- **Using `latest` tag in kustomization.yaml:** Defeats traceability. Every deployment must map to a specific commit SHA.
- **Committing with a PAT instead of GITHUB_TOKEN:** PAT-based commits trigger subsequent workflows, risking infinite loops. GITHUB_TOKEN commits do not trigger push-based workflows.
- **Running `kustomize edit set image` in the base directory:** The base deployment.yaml has the original image reference; the overlay is where environment-specific image overrides belong.
- **Hardcoding the SHA in the workflow:** Use `${{ github.sha }}` and `cut -c1-7` to derive the short SHA dynamically.
- **Skipping the `git diff --staged --quiet` check:** If no files changed (e.g., re-run of the same commit), the commit step should be a no-op rather than failing.
- **Modifying overlay for tag-only push (v\*.\*.\*):** The workflow triggers on both `push: branches: [master]` and `push: tags: ['v*.*.*']`. The manifest update should only happen for branch pushes to `master`, not for tag pushes (which are release artifacts, not deployments).

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Image tag update in YAML | Custom sed/awk to replace `newTag:` | `kustomize edit set image` | Handles YAML formatting, multi-line values, comments; official CLI command |
| Git commit automation | Custom shell script with error handling | `git config && git add && git commit && git push` in workflow step | Simple enough for 5 lines; no third-party action needed |
| SHA tag generation | Custom hashing or tag derivation | `docker/metadata-action` `type=sha,prefix=sha-` | Already generates correct format; just derive matching format in update job |
| ArgoCD sync trigger | Custom API call to ArgoCD | ArgoCD's built-in Git polling (3 min) | Polling is sufficient for 5-minute SLA; webhook requires internet-accessible ArgoCD |

**Key insight:** The entire GitOps loop closure is ~30 lines of workflow YAML added to an existing file. No new tools, no new repos, no complex infrastructure. The hard work was done in Phases 23-26.

## Common Pitfalls

### Pitfall 1: Infinite Workflow Loop
**What goes wrong:** The `update-manifests` job commits to `master`, which triggers `container-images.yml` again, which builds images and commits again, forever.
**Why it happens:** If using a PAT or misconfigured token, push events can re-trigger the same workflow.
**How to avoid:** Use `GITHUB_TOKEN` (not a PAT) for the git push. GitHub Actions does not trigger `push`-based workflows for commits made by `GITHUB_TOKEN`. As a belt-and-suspenders measure, include `[skip ci]` in the commit message.
**Warning signs:** Multiple identical workflow runs in quick succession; the Actions tab shows a cascade of runs.

### Pitfall 2: SHA Tag Mismatch
**What goes wrong:** The tag in `kustomization.yaml` (e.g., `sha-860c190`) does not match any tag pushed to ghcr.io, causing `ImagePullBackOff` in the cluster.
**Why it happens:** The `docker/metadata-action` uses a 7-character short SHA by default, but the update-manifests job uses a different length or format.
**How to avoid:** Use `echo '${{ github.sha }}' | cut -c1-7` to produce the same 7-character short SHA. Verify by checking `docker/metadata-action` step output in a previous run.
**Warning signs:** Pods in `ImagePullBackOff`; `kubectl describe pod` shows "manifest unknown" or "not found" for the image tag.

### Pitfall 3: Race Condition on Push
**What goes wrong:** The `update-manifests` job checks out the repo at the triggering commit, but another commit lands on `master` before the push, causing a `push rejected (non-fast-forward)` error.
**Why it happens:** In a high-traffic repo, multiple pushes to `master` can overlap.
**How to avoid:** For this project (single developer), this is extremely unlikely. If it becomes an issue, add `git pull --rebase` before `git push`, or use a retry step. The workflow will simply fail and the next push will succeed.
**Warning signs:** `update-manifests` job fails with "non-fast-forward" error.

### Pitfall 4: Tag Push Triggers Manifest Update
**What goes wrong:** A release tag push (`v1.0.0`) triggers the workflow, and the `update-manifests` job updates overlays with the SHA of the tag commit, overwriting the latest `master` SHA tag.
**Why it happens:** The workflow triggers on both `push: branches: [master]` and `push: tags: ['v*.*.*']`.
**How to avoid:** Add an `if` condition to the `update-manifests` job: `if: github.ref == 'refs/heads/master'`. This ensures manifest updates only happen for branch pushes, not tag pushes.
**Warning signs:** Overlay kustomization.yaml being updated on tag pushes with non-master SHAs.

### Pitfall 5: Overlay `newName` Still Points to Local Image
**What goes wrong:** The overlay has `newName: apiservice` (local Docker name) instead of `newName: ghcr.io/baotoq/micro-commerce/apiservice` (registry path). ArgoCD syncs the manifest, but the pod tries to pull `apiservice:sha-860c190` from Docker Hub instead of ghcr.io.
**Why it happens:** The current overlays were set up for local kind development where images are loaded directly. CI/CD images live in ghcr.io.
**How to avoid:** `kustomize edit set image apiservice=ghcr.io/baotoq/micro-commerce/apiservice:sha-xxx` updates both `newName` and `newTag` simultaneously. This is the correct command to use.
**Warning signs:** Pods in `ImagePullBackOff` with "repository does not exist" error.

### Pitfall 6: Bootstrap Script Conflicts with CI/CD Overlays
**What goes wrong:** The bootstrap script builds local images tagged `dev` and the overlays reference `newTag: dev`. After CI/CD updates the overlay to `sha-xxx`, the bootstrap script no longer works because the local kind cluster doesn't have `ghcr.io/...` images.
**Why it happens:** The overlay serves two purposes: local development and CI/CD deployment.
**How to avoid:** Accept that the overlay in Git reflects the last CI/CD deployment. For local development, the bootstrap script can either: (a) run `kustomize edit set image` to reset overlays to local tags after applying, or (b) be updated to also tag local images with the same format. The simplest approach is to accept that the overlay reflects CI/CD state and the bootstrap script's local image loading works independently (ArgoCD is not used in local kind development without the full CI/CD pipeline).
**Warning signs:** Bootstrap script succeeds but ArgoCD shows OutOfSync because overlay tags don't match loaded images.

## Code Examples

Verified patterns from official sources:

### Complete `update-manifests` Job
```yaml
# Source: Standard GitOps pattern from Kustomize + GitHub Actions docs
update-manifests:
  name: Update Kustomize Manifests
  needs: [build-web, build-apiservice, build-gateway]
  if: github.ref == 'refs/heads/master'
  runs-on: ubuntu-latest
  permissions:
    contents: write
  steps:
    - name: Checkout
      uses: actions/checkout@v4

    - name: Update image tags in dev overlay
      run: |
        SHORT_SHA="sha-$(echo '${{ github.sha }}' | cut -c1-7)"
        echo "Updating image tags to: $SHORT_SHA"

        cd infra/k8s/overlays/dev/apiservice
        kustomize edit set image apiservice=ghcr.io/baotoq/micro-commerce/apiservice:$SHORT_SHA

        cd ../gateway
        kustomize edit set image gateway=ghcr.io/baotoq/micro-commerce/gateway:$SHORT_SHA

        cd ../web
        kustomize edit set image web=ghcr.io/baotoq/micro-commerce/web:$SHORT_SHA

    - name: Commit and push
      run: |
        git config user.name "github-actions[bot]"
        git config user.email "github-actions[bot]@users.noreply.github.com"
        git add infra/k8s/overlays/dev/
        git diff --staged --quiet && echo "No changes to commit" && exit 0
        git commit -m "[skip ci] chore: update image tags to sha-$(echo '${{ github.sha }}' | cut -c1-7)"
        git push
```

### Kustomize Edit Set Image Command
```bash
# Source: https://github.com/kubernetes-sigs/kustomize/blob/master/examples/image.md
# Syntax: kustomize edit set image <oldName>=<newRegistry>/<newName>:<newTag>

# Before (in kustomization.yaml):
# images:
# - name: apiservice
#   newName: apiservice
#   newTag: dev

kustomize edit set image apiservice=ghcr.io/baotoq/micro-commerce/apiservice:sha-860c190

# After (in kustomization.yaml):
# images:
# - name: apiservice
#   newName: ghcr.io/baotoq/micro-commerce/apiservice
#   newTag: sha-860c190
```

### SHA Tag Derivation
```bash
# docker/metadata-action with type=sha,prefix=sha- produces: sha-860c190 (7 chars)
# To match in shell:
SHORT_SHA="sha-$(echo '${{ github.sha }}' | cut -c1-7)"
# For full SHA: ${{ github.sha }} = 860c1904a1ce19322e91ac35af1ab07466440c37
# Short: sha-860c190
```

### Verifying the Loop End-to-End
```bash
# 1. Check the latest image tag in the overlay
cat infra/k8s/overlays/dev/apiservice/kustomization.yaml
# images:
# - name: apiservice
#   newName: ghcr.io/baotoq/micro-commerce/apiservice
#   newTag: sha-860c190

# 2. Check ArgoCD sees the updated Application
kubectl get applications -n argocd -o jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.status.sync.status}{"\n"}{end}'

# 3. Check the running pod's image matches
kubectl get pods -n micro-commerce -o jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.spec.containers[0].image}{"\n"}{end}'
# apiservice-xxx   ghcr.io/baotoq/micro-commerce/apiservice:sha-860c190
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Separate config repo for manifests | Single repo (app + infra in same repo) | Ongoing pattern | Simpler for single-team projects; `GITHUB_TOKEN` can commit to same repo |
| Custom webhook handlers for ArgoCD | ArgoCD built-in 3-min polling or `/api/webhook` | ArgoCD 1.x+ | No custom infrastructure needed for GitOps sync trigger |
| `sed -i` to replace image tags | `kustomize edit set image` CLI | Kustomize 3.x+ | YAML-aware, handles formatting, official tool |
| PAT-based commits in CI | GITHUB_TOKEN with `contents: write` | GitHub Actions 2021+ | Built-in infinite loop prevention; no token management overhead |

**Deprecated/outdated:**
- Using `sed` to modify Kustomize files: fragile, not YAML-aware
- Separate GitOps repository for manifests: adds complexity for single-team projects; single-repo is standard for small projects
- ArgoCD Image Updater (write-back): designed for auto-updating to latest tags; not needed when CI explicitly sets the tag

## Open Questions

1. **Bootstrap script compatibility after CI/CD overlay update**
   - What we know: The bootstrap script builds local images tagged `dev` and uses overlays with `newTag: dev`. After CI/CD, the overlay will have `newTag: sha-xxx` pointing to ghcr.io.
   - What's unclear: Should the bootstrap script be updated to handle this divergence, or is it acceptable that the overlay in Git reflects CI/CD state?
   - Recommendation: Accept the divergence. The bootstrap script's Step 10-11 (build/load images) and Step 12 (apply ArgoCD root app) work independently. For local development, the user can either: (a) let the bootstrap script work as-is (ArgoCD will sync from Git, pulling ghcr.io images if the cluster has internet), or (b) after bootstrap, manually reset overlays with `kustomize edit set image apiservice=apiservice:dev` etc. This is a local dev workflow concern, not a CI/CD concern. Defer to implementation.

2. **ArgoCD sync latency vs 5-minute SLA**
   - What we know: ArgoCD polls Git every 3 minutes by default. The success criterion requires a new pod within 5 minutes of CI workflow completing.
   - What's unclear: Whether 3-minute polling + pod rollout time fits within 5 minutes.
   - Recommendation: 3-minute polling + ~30s for pod rollout = ~3.5 minutes typical, well within the 5-minute SLA. If tighter latency is needed later, configure a GitHub webhook to ArgoCD's `/api/webhook` endpoint. Not needed for this phase.

3. **`contents: write` permission scope**
   - What we know: The `update-manifests` job needs `permissions: contents: write` to push commits. The build jobs currently have `permissions: contents: read, packages: write`.
   - What's unclear: Whether job-level permissions can differ from workflow-level permissions.
   - Recommendation: GitHub Actions supports per-job permissions. Set `contents: write` only on the `update-manifests` job. The build jobs keep their existing `contents: read` + `packages: write`.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | Manual validation via GitHub Actions run + kubectl (infrastructure/CI phase, no unit test framework) |
| Config file | `.github/workflows/container-images.yml` |
| Quick run command | Check workflow run status in GitHub Actions tab |
| Full suite command | Push to master, verify: (1) workflow completes, (2) overlay kustomization.yaml updated in Git, (3) ArgoCD syncs, (4) pods running new image |

### Phase Requirements to Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| CICD-02 | CI commits updated SHA image tags to Kustomize dev overlay | smoke | `git log --oneline -1 -- infra/k8s/overlays/dev/` (verify [skip ci] commit) | N/A -- Git history |
| CICD-02 | Image tag in overlay matches commit SHA | smoke | `grep newTag infra/k8s/overlays/dev/apiservice/kustomization.yaml` (verify sha-XXX format) | N/A -- file content |
| CICD-02 | ArgoCD syncs updated manifests | smoke | `kubectl get applications -n argocd -o jsonpath='{.items[*].status.sync.status}'` | N/A -- kubectl |
| CICD-02 | Pod runs image matching commit SHA | smoke | `kubectl get pods -n micro-commerce -o jsonpath='{.items[*].spec.containers[0].image}'` | N/A -- kubectl |
| CICD-02 | Workflow does not trigger infinite loop | smoke | Check GitHub Actions tab shows no cascading runs after the [skip ci] commit | N/A -- GitHub UI |

### Sampling Rate
- **Per task commit:** Verify workflow YAML syntax with `act --dryrun` or visual review
- **Per wave merge:** Push to master, verify full loop: build -> commit -> ArgoCD sync -> pod update
- **Phase gate:** Both success criteria verified before `/gsd:verify-work`

### Wave 0 Gaps
None -- this phase modifies a GitHub Actions workflow file and Kustomize overlays. No test framework infrastructure needed.

## Sources

### Primary (HIGH confidence)
- [/kubernetes-sigs/kustomize](https://github.com/kubernetes-sigs/kustomize/blob/master/examples/image.md) - `kustomize edit set image` command syntax and behavior verified via Context7
- [docker/metadata-action@v5](https://github.com/docker/metadata-action) - SHA tag format: `type=sha,prefix=sha-` produces `sha-<7chars>` by default
- [GitHub Actions: Skipping workflow runs](https://docs.github.com/en/actions/managing-workflow-runs-and-deployments/managing-workflow-runs/skipping-workflow-runs) - `[skip ci]` in commit message skips push/PR-triggered workflows
- [GitHub Actions: GITHUB_TOKEN](https://github.com/orgs/community/discussions/25702) - Events triggered by GITHUB_TOKEN do not create new workflow runs (except workflow_dispatch/repository_dispatch)
- [ArgoCD Git Webhook Configuration](https://argo-cd.readthedocs.io/en/latest/operator-manual/webhook/) - Webhook endpoint at `/api/webhook` for immediate sync (optional, not needed for this phase)
- [ArgoCD Auto Sync](https://argo-cd.readthedocs.io/en/stable/user-guide/auto_sync/) - Default 3-minute polling interval; selfHeal and automated sync configuration

### Secondary (MEDIUM confidence)
- [GitOps CI/CD patterns with GitHub Actions + ArgoCD](https://medium.com/@mehmetkanus17/argocd-github-actions-a-complete-gitops-ci-cd-workflow-for-kubernetes-applications-ed2f91d37641) - Workflow structure with `needs:` dependency for update-manifest job
- [Automation with GitHub Actions and Kustomize](https://gap.gjensidige.io/docs/guides/ci-automation-with-kustomize) - `kustomize edit set image` in CI/CD pipelines pattern

### Tertiary (LOW confidence)
- None -- all findings verified with official documentation

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - No new tools needed; existing `container-images.yml`, Kustomize, and ArgoCD are all already in place. Only addition is a new job in an existing workflow.
- Architecture: HIGH - The `update-manifests` job pattern is the standard GitOps CI/CD pattern, verified across multiple official and community sources. `kustomize edit set image` syntax verified via Context7.
- Pitfalls: HIGH - Infinite loop prevention via GITHUB_TOKEN is documented by GitHub. SHA format alignment is straightforward. Race condition is theoretical for single-developer repo.

**Research date:** 2026-03-02
**Valid until:** 2026-04-02 (30 days -- GitHub Actions and Kustomize are stable; ArgoCD sync behavior is well-established)
