# Deploy — GitOps to local OrbStack Kubernetes

Full GitOps deployment of the Micro Commerce stack, reconciled by **ArgoCD** from this repo.
Design: `docs/superpowers/specs/2026-06-13-argocd-gitops-design.md` ·
Plan: `docs/superpowers/plans/2026-06-13-argocd-gitops.md`

## Prerequisites

- OrbStack with Kubernetes enabled (`orb start k8s`)
- `kubectl`, `helm`, `kustomize` (or `kubectl kustomize`), `kubeconform`
- A GitHub repo with the two GHCR packages either **public** or reachable via a pull secret

## One-time bootstrap

ArgoCD cannot deploy itself from nothing, so install it imperatively once:

```bash
# (optional) for PRIVATE GHCR packages, export a PAT with read:packages first:
# export GHCR_USER=<github-user> GHCR_PAT=<pat>
./deploy/bootstrap/install-argocd.sh
```

This: starts k8s, applies the CoreDNS rewrite (`deploy/k8s/coredns-custom.yaml`) so pods and the
browser resolve the **same** `*.k8s.orb.local` hostnames, installs ArgoCD (non-HA), optionally
creates the GHCR pull secret, and applies the App-of-Apps root. ArgoCD then reconciles everything.

## Topology (sync waves)

```
root (app-of-apps)
  ├─ wave -2  dapr            Helm dapr/dapr → dapr-system (prune only, no selfHeal)
  ├─ wave  0  infra           postgres, redis, keycloak, azurite
  ├─ wave  1  dapr-components  pubsub.redis Component (needs Dapr CRDs + Redis)
  └─ wave  2  app             catalog-api (+Dapr sidecar) + web
```

Waves order the child Applications; the next wave waits until the current wave is Healthy.

## URLs

- Web: `http://web.micro-commerce.k8s.orb.local`
- Keycloak: `http://keycloak.micro-commerce.k8s.orb.local`  (issuer `…/realms/microcommerce`)
- Azurite blob: `http://azurite.micro-commerce.k8s.orb.local/devstoreaccount1`
- ArgoCD UI: `kubectl -n argocd port-forward svc/argocd-server 8090:443` → https://localhost:8090

> Verify the exact LoadBalancer domain format on your cluster (`kubectl -n micro-commerce get svc`).
> If it isn't `<svc>.micro-commerce.k8s.orb.local`, update the issuer/web/azurite hosts in
> `deploy/k8s/**` and the realm `redirectUris`, then re-sync.

## Image GitOps loop

Push to `master` → `.github/workflows/gitops.yml`:
1. `validate` — `kustomize build | kubeconform` on every overlay.
2. `build` — build + push `ghcr.io/<owner>/micro-commerce/{catalog-api,web}:sha-<sha>`.
3. `bump-gitops` — `kustomize edit set image` in `deploy/k8s/app/overlays/local` + commit `[skip ci]`.

ArgoCD detects the bump commit and syncs the new immutable SHA tags. The `GITHUB_TOKEN` push does
not retrigger CI (recursion guard); `paths-ignore` + `[skip ci]` are belt-and-suspenders.

**First run:** make the two GHCR packages public (Package settings → Danger Zone → Public), or set
`GHCR_USER`/`GHCR_PAT` before bootstrap.

## Maintenance notes

- **Realm sync:** `deploy/k8s/infra/keycloak/microcommerce-realm.json` is a copy of
  `src/AppHost/Realms/microcommerce-realm.json`. Keep them in sync; the k8s copy must include the
  `http://web.micro-commerce.k8s.orb.local/*` redirect URI + web origin.
- **CoreDNS rewrite** is applied by the bootstrap script (not the namespaced infra kustomization,
  which would force it into the wrong namespace). **Re-verify after `orb restart k8s`** — OrbStack
  may reconcile CoreDNS config on restart.
- **Dapr version:** `deploy/argocd/apps/dapr.yaml` pins a `targetRevision`; confirm the current GA
  with `helm search repo dapr --versions` and bump as needed.
- **Secrets are DEV-ONLY** (committed plaintext, mirroring `AppHost.cs` dev values). For any real
  cluster, switch to Sealed Secrets / External Secrets.
- **PV reclaim quirk:** OrbStack frees deleted PVs' disk only after `orb restart`.
- **Single replica** for catalog-api: EF Core auto-migrates on boot; >1 replica races migrations.
