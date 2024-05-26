# Fluxcd

- **[`Flux CD`](https://fluxcd.io/)** - Flux CD automates Kubernetes deployment from Git, ensuring continuous delivery seamlessly.

## Bootstrap FluxCD

```shell
flux bootstrap github \
  --token-auth \
  --owner=baotoq \
  --repository=micro-commerce \
  --branch=master \
  --path=deploy/fluxcd/clusters \
  --personal
```
