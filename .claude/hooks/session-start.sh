#!/bin/bash
# SessionStart hook for Claude Code on the web: installs the pinned toolchain and
# restores dependencies so builds, lint and tests work in a fresh cloud container.
# Locally, mise.toml handles the toolchain (see README), so this exits early.
set -euo pipefail

if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

cd "$CLAUDE_PROJECT_DIR"

FRONTEND_DIR=src/micro-commerce
NODE_HOME=/opt/node26
DOTNET_HOME=/opt/dotnet

persist_env() {
  if [ -n "${CLAUDE_ENV_FILE:-}" ]; then
    echo "$1" >> "$CLAUDE_ENV_FILE"
  fi
}

# --- .NET SDK -----------------------------------------------------------------
# dot.net / builds.dotnet.microsoft.com are blocked by the cloud network policy and
# Ubuntu's own feed only carries the 10.0.1xx band, but global.json needs 10.0.4xx.
# packages.microsoft.com is reachable, so take the SDK .debs from its Debian 12 feed.
# They're extracted rather than apt-installed: dotnet-runtime-deps-10.0 (an empty
# dependency stub) asks for Debian's libicu72+, while Ubuntu 24.04 has libicu74,
# which .NET finds on its own.
sdk_version=$(python3 -c 'import json; print(json.load(open("global.json"))["sdk"]["version"])')
sdk_band=${sdk_version%??}   # 10.0.400 -> 10.0.4
if ! "$DOTNET_HOME/dotnet" --list-sdks 2>/dev/null | grep -q "^${sdk_band}"; then
  echo "Installing .NET SDK ${sdk_band}xx from packages.microsoft.com..."
  feed=https://packages.microsoft.com/debian/12/prod
  debs=$(curl --proto '=https' --tlsv1.2 -fsSL "$feed/dists/bookworm/main/binary-amd64/Packages.gz" | gunzip | python3 -c '
import re, sys
band = sys.argv[1]
pkgs = {}
for stanza in sys.stdin.read().split("\n\n"):
    f = dict(l.split(": ", 1) for l in stanza.splitlines() if ": " in l and not l.startswith(" "))
    if "Package" in f:
        pkgs.setdefault(f["Package"], []).append(f)
key = lambda f: [int(n) for n in re.findall(r"\d+", f["Version"])]
upstream = lambda f: f["Version"].split("-")[0]
sdk = max((f for f in pkgs["dotnet-sdk-10.0"] if f["Version"].startswith(band)), key=key)
runtime = re.search(r"dotnet-runtime-10.0 \(>= ([\d.]+)\)", sdk["Depends"]).group(1)
print(sdk["Filename"])
for name in ["dotnet-host", "dotnet-hostfxr-10.0", "dotnet-runtime-10.0", "aspnetcore-runtime-10.0",
             "dotnet-targeting-pack-10.0", "dotnet-apphost-pack-10.0", "aspnetcore-targeting-pack-10.0"]:
    print(max((f for f in pkgs[name] if upstream(f) == runtime), key=key)["Filename"])
' "$sdk_band")
  tmp=$(mktemp -d)
  for deb in $debs; do
    curl --proto '=https' --tlsv1.2 -fsSL -o "$tmp/pkg.deb" "$feed/$deb"
    dpkg-deb -x "$tmp/pkg.deb" "$tmp/root"
  done
  rm -rf "$DOTNET_HOME" && mkdir -p "$(dirname "$DOTNET_HOME")"
  mv "$tmp/root/usr/share/dotnet" "$DOTNET_HOME"
  rm -rf "$tmp"
fi
ln -sf "$DOTNET_HOME/dotnet" /usr/local/bin/dotnet
persist_env "export DOTNET_ROOT=${DOTNET_HOME}"
persist_env "export DOTNET_CLI_TELEMETRY_OPTOUT=1"
persist_env "export DOTNET_NOLOGO=1"
persist_env 'export PATH="$PATH:$HOME/.dotnet/tools"'
export DOTNET_ROOT="$DOTNET_HOME" DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1

dotnet tool restore
dotnet restore MicroCommerce.slnx

# --- Node + pnpm ----------------------------------------------------------------
# package.json's devEngines asks for a newer Node than the image ships.
node_range=$(python3 -c 'import json; print(json.load(open("'"$FRONTEND_DIR"'/package.json"))["devEngines"]["runtime"]["version"])')
node_min=${node_range#>=}
node_major=${node_min%%.*}
node_ok() {
  "$1" -e 'const [a,b,c]=process.versions.node.split(".").map(Number),[x,y,z]=process.argv[1].split(".").map(Number);process.exit(a!==x?+(a<x):b!==y?+(b<y):+(c<z))' "$node_min" 2>/dev/null
}
if ! node_ok "$NODE_HOME/bin/node"; then
  node_version=$(curl --proto '=https' --tlsv1.2 -fsSL https://nodejs.org/dist/index.json \
    | python3 -c 'import json,sys; print(next(r["version"] for r in json.load(sys.stdin) if r["version"].startswith("v'"$node_major"'.")))')
  echo "Installing Node ${node_version}..."
  rm -rf "$NODE_HOME" && mkdir -p "$NODE_HOME"
  curl --proto '=https' --tlsv1.2 -fsSL "https://nodejs.org/dist/${node_version}/node-${node_version}-linux-x64.tar.xz" \
    | tar -xJ -C "$NODE_HOME" --strip-components=1
fi
export PATH="$NODE_HOME/bin:$PATH"
persist_env "export PATH=\"$NODE_HOME/bin:\$PATH\""

# Node 25+ no longer bundles corepack, so install the pinned pnpm with npm.
pnpm_version=$(python3 -c 'import json; print(json.load(open("'"$FRONTEND_DIR"'/package.json"))["packageManager"].split("@")[1])')
if [ "$(pnpm --version 2>/dev/null)" != "$pnpm_version" ]; then
  npm install -g --silent "pnpm@${pnpm_version}"
fi

pnpm --dir "$FRONTEND_DIR" install --frozen-lockfile

# --- Docker ---------------------------------------------------------------------
# Functional tests (Testcontainers) and integration tests (Aspire) need a container
# runtime. The image ships dockerd but doesn't start it.
if ! docker info >/dev/null 2>&1; then
  echo "Starting dockerd..."
  nohup dockerd >/var/log/dockerd.log 2>&1 &
  for _ in $(seq 1 30); do
    docker info >/dev/null 2>&1 && break
    sleep 1
  done
  docker info >/dev/null 2>&1 || { echo "dockerd failed to start, see /var/log/dockerd.log" >&2; exit 1; }
fi
