# ArgoCD Cluster Bootstrapping - Project Structure

Complete directory tree and file organization for the multi-repository GitOps environment.

---

## Skill Structure

```
.claude/skills/argocd-cluster-bootstrapping/
├── SKILL.md                          # Main entry point
└── references/
    ├── summary.md                    # Overview and scope
    ├── workflow.md                   # Step-by-step bootstrap process
    ├── tools.md                      # CLI commands and scripts
    ├── guidance.md                   # Best practices and troubleshooting
    ├── architecture.md               # System architecture deep dive
    ├── project-structure.md          # This file
    └── templates/
        ├── README.md                 # Template usage guide
        ├── cluster-secret.yaml       # Cluster registration secret
        ├── argocd-project.yaml       # RBAC project definition
        ├── applicationset-cluster-generator.yaml
        ├── applicationset-matrix-generator.yaml
        ├── values-base.yaml          # Base Helm values
        ├── values-dev.yaml           # Development values
        ├── values-prd.yaml           # Production values
        ├── external-secret-store.yaml # External Secrets config
        └── bootstrap-script.sh       # Automation script
```

---

## Infrastructure Repository Structure

```
infra-team/
│
├── applicationset/                           # ApplicationSet definitions
│   │
│   ├── kube-addons/                         # Platform components
│   │   ├── cert-manager.yaml                # 📄 Certificate management
│   │   ├── external-secrets.yaml            # 📄 Secret management
│   │   ├── external-dns.yaml                # 📄 DNS automation
│   │   ├── ingress-nginx.yaml               # 📄 Ingress controller
│   │   ├── prometheus-stack.yaml            # 📄 Monitoring
│   │   ├── loki.yaml                        # 📄 Logging
│   │   ├── grafana.yaml                     # 📄 Visualization
│   │   ├── argocd-image-updater.yaml        # 📄 Image automation
│   │   └── kargo.yaml                       # 📄 Progressive delivery
│   │
│   ├── applications/                        # Business applications
│   │   ├── cafehyna/
│   │   │   ├── api.yaml
│   │   │   ├── web.yaml
│   │   │   └── workers.yaml
│   │   ├── loyalty/
│   │   │   └── loyalty-api.yaml
│   │   └── painelclientes/
│   │       └── panel-web.yaml
│   │
│   └── master-applicationset.yaml           # 📄 Orchestrator
│
├── argocd-clusters/                         # Cluster registration
│   │
│   │   # Development clusters
│   ├── cafehyna-dev.yaml                   # 📄 labels: env=dev, node-type=spot
│   ├── loyalty-dev.yaml                    # 📄 labels: env=dev, node-type=spot
│   ├── sonora-dev.yaml                     # 📄 labels: env=dev, node-type=spot
│   │
│   │   # Homologation clusters
│   ├── cafehyna-hlg.yaml                   # 📄 labels: env=hlg, node-type=mixed
│   ├── loyalty-hlg.yaml                    # 📄 labels: env=hlg, node-type=mixed
│   │
│   │   # Production clusters
│   ├── cafehyna-prd.yaml                   # 📄 labels: env=prd, node-type=standard
│   ├── loyalty-prd.yaml                    # 📄 labels: env=prd, node-type=standard
│   │
│   │   # Hub cluster
│   └── cafehyna-hub.yaml                   # 📄 labels: env=hub, tier=platform
│
├── argocd-projects/                         # RBAC definitions
│   │
│   │   # Platform projects
│   ├── platform.yaml                       # 📄 Platform team project
│   │
│   │   # Cluster-specific projects
│   ├── cafehyna-dev.yaml                   # 📄 Dev cluster project
│   ├── cafehyna-hlg.yaml                   # 📄 HLG cluster project
│   ├── cafehyna-prd.yaml                   # 📄 PRD cluster project
│   ├── loyalty-dev.yaml
│   ├── loyalty-hlg.yaml
│   └── loyalty-prd.yaml
│
├── argocd/                                  # ArgoCD configuration
│   ├── argocd-cm.yaml                      # 📄 ConfigMap
│   ├── argocd-rbac-cm.yaml                 # 📄 RBAC ConfigMap
│   ├── argocd-secret.yaml                  # 📄 Secrets
│   ├── argocd-notifications-cm.yaml        # 📄 Notifications config
│   └── argocd-notifications-secret.yaml    # 📄 Notifications secrets
│
├── applicationset-templates/                # Reusable templates
│   ├── multi-source-helm.yaml              # 📄 Standard multi-source pattern
│   ├── cluster-generator.yaml              # 📄 Cluster generator base
│   ├── matrix-generator.yaml               # 📄 Matrix generator base
│   └── progressive-rollout.yaml            # 📄 Progressive delivery
│
├── helm-charts/                             # Custom Helm charts
│   ├── common/                             # Shared templates
│   │   └── templates/
│   │       ├── _helpers.tpl
│   │       ├── deployment.yaml
│   │       ├── service.yaml
│   │       └── ingress.yaml
│   └── applications/                       # Business app charts
│       ├── cafehyna-api/
│       │   ├── Chart.yaml
│       │   ├── values.yaml
│       │   └── templates/
│       └── loyalty-api/
│
└── scripts/                                 # Operational scripts
    ├── bootstrap-cluster.sh                # 📄 Cluster bootstrap
    ├── validate-cluster.sh                 # 📄 Cluster validation
    ├── cleanup-cluster.sh                  # 📄 Cluster removal
    ├── sync-all-apps.sh                    # 📄 Bulk sync
    └── check-health.sh                     # 📄 Health check
```

---

## Values Repository Structure

```
argo-cd-helm-values/
│
└── kube-addons/
    │
    ├── cert-manager/                        # Certificate management
    │   ├── base/
    │   │   └── values.yaml                 # 📄 Base values (all envs)
    │   ├── cafehyna-dev/
    │   │   └── values.yaml                 # 📄 Dev: spot tolerations
    │   ├── cafehyna-hlg/
    │   │   └── values.yaml                 # 📄 HLG: 2 replicas
    │   ├── cafehyna-prd/
    │   │   └── values.yaml                 # 📄 PRD: 3 replicas, HA
    │   ├── loyalty-dev/
    │   │   └── values.yaml
    │   ├── loyalty-hlg/
    │   │   └── values.yaml
    │   └── loyalty-prd/
    │       └── values.yaml
    │
    ├── external-secrets/                    # Secret management
    │   ├── base/
    │   │   └── values.yaml
    │   ├── cafehyna-dev/
    │   │   └── values.yaml
    │   ├── cafehyna-hlg/
    │   │   └── values.yaml
    │   └── cafehyna-prd/
    │       └── values.yaml
    │
    ├── ingress-nginx/                       # Ingress controller
    │   ├── base/
    │   │   └── values.yaml                 # 📄 Metrics, service config
    │   ├── cafehyna-dev/
    │   │   └── values.yaml                 # 📄 Spot tolerations, 1 replica
    │   ├── cafehyna-hlg/
    │   │   └── values.yaml                 # 📄 Mixed nodes, 2 replicas
    │   └── cafehyna-prd/
    │       └── values.yaml                 # 📄 Standard nodes, 3 replicas, PDB
    │
    ├── prometheus-stack/                    # Monitoring
    │   ├── base/
    │   │   └── values.yaml                 # 📄 ServiceMonitors, rules
    │   ├── cafehyna-dev/
    │   │   └── values.yaml                 # 📄 3d retention, 10Gi storage
    │   ├── cafehyna-hlg/
    │   │   └── values.yaml                 # 📄 7d retention, 50Gi storage
    │   └── cafehyna-prd/
    │       └── values.yaml                 # 📄 30d retention, 200Gi storage
    │
    ├── loki/                                # Logging
    │   ├── base/
    │   │   └── values.yaml
    │   ├── cafehyna-dev/
    │   │   └── values.yaml
    │   ├── cafehyna-hlg/
    │   │   └── values.yaml
    │   └── cafehyna-prd/
    │       └── values.yaml
    │
    ├── external-dns/                        # DNS automation
    │   ├── base/
    │   │   └── values.yaml
    │   ├── cafehyna-dev/
    │   │   └── values.yaml
    │   └── cafehyna-prd/
    │       └── values.yaml
    │
    ├── grafana/                             # Visualization
    │   ├── base/
    │   │   └── values.yaml
    │   └── cafehyna-hub/
    │       └── values.yaml
    │
    └── ... (28+ components)
```

---

## Documentation Repository Structure

```
docs/
│
├── argocd/                                  # ArgoCD documentation
│   ├── argocd-architecture.md              # 📄 System architecture
│   ├── argocd-applicationsets.md           # 📄 ApplicationSet guide
│   ├── argocd-multi-source.md              # 📄 Multi-source patterns
│   ├── argocd-rbac.md                      # 📄 RBAC configuration
│   ├── argocd-notifications.md             # 📄 Notification setup
│   └── argocd-troubleshooting.md           # 📄 Common issues
│
├── clusters/                                # Cluster documentation
│   ├── cafehyna-dev.md                     # 📄 Dev cluster profile
│   ├── cafehyna-hlg.md                     # 📄 HLG cluster profile
│   ├── cafehyna-prd.md                     # 📄 PRD cluster profile
│   ├── cafehyna-hub.md                     # 📄 Hub cluster profile
│   └── cluster-inventory.md                # 📄 Full inventory
│
├── components/                              # Component documentation
│   ├── cert-manager.md                     # 📄 Certificate setup
│   ├── external-secrets.md                 # 📄 Secret management
│   ├── ingress-nginx.md                    # 📄 Ingress configuration
│   ├── prometheus-stack.md                 # 📄 Monitoring setup
│   └── loki.md                             # 📄 Logging setup
│
├── runbooks/                                # Operational runbooks
│   ├── cluster-bootstrap.md                # 📄 Bootstrap procedure
│   ├── cluster-upgrade.md                  # 📄 Upgrade procedure
│   ├── disaster-recovery.md                # 📄 DR procedures
│   └── incident-response.md                # 📄 Incident handling
│
└── getting-started.md                       # 📄 Quick start guide
```

---

## File Counts Summary

| Category | Count | Description |
|----------|-------|-------------|
| ApplicationSets | 40+ | Dynamic application deployment |
| Cluster Secrets | 9 | Registered clusters |
| ArgoCD Projects | 10+ | RBAC boundaries |
| Values Files | 200+ | Component × cluster configurations |
| Documentation | 50+ | Guides and runbooks |
| Scripts | 100+ | Operational automation |

---

## Naming Conventions

### Files
```
Pattern: <resource-type>-<identifier>.yaml

Examples:
  applicationset/kube-addons/ingress-nginx.yaml
  argocd-clusters/cafehyna-dev.yaml
  argocd-projects/platform.yaml
```

### Directories
```
Pattern: <category>/<subcategory>/<identifier>/

Examples:
  kube-addons/ingress-nginx/cafehyna-dev/
  applications/cafehyna/api/
```

### Values Files
```
Pattern: kube-addons/<component>/<cluster>/values.yaml

Examples:
  kube-addons/ingress-nginx/base/values.yaml
  kube-addons/ingress-nginx/cafehyna-dev/values.yaml
  kube-addons/prometheus-stack/cafehyna-prd/values.yaml
```
