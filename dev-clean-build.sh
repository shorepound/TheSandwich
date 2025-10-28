#!/usr/bin/env bash
set -euo pipefail

# dev-clean-build.sh
# Stops dev servers, clears common caches and build artifacts, restores dependencies, and performs fresh builds
# Usage: ./dev-clean-build.sh [--full]
#   --full : also removes frontend node_modules (use when you want a truly clean npm install)

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$ROOT_DIR"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log() { echo -e "${BLUE}[INFO]${NC} $1"; }
ok()  { echo -e "${GREEN}[OK]${NC} $1"; }
warn(){ echo -e "${YELLOW}[WARN]${NC} $1"; }
err() { echo -e "${RED}[ERR]${NC} $1"; }

FULL_CLEAN=false
if [ "${1:-}" = "--full" ]; then
  FULL_CLEAN=true
fi

log "Stopping dev services before cleaning..."
if [ -x ./dev-stop-all.sh ]; then
  ./dev-stop-all.sh || warn "dev-stop-all.sh returned non-zero; continuing"
else
  warn "dev-stop-all.sh not executable or missing; you may need to stop services manually"
fi

log "Clearing .NET/NuGet caches..."
if command -v dotnet >/dev/null 2>&1; then
  dotnet nuget locals all --clear || warn "dotnet nuget locals failed"
  dotnet clean --verbosity minimal || warn "dotnet clean failed"
else
  warn ".NET SDK not found in PATH; skipping dotnet cleanup"
fi

log "Removing common build artifacts (bin/ obj / dist)..."
# Top-level artifacts
rm -rf ./bin ./obj 2>/dev/null || true
rm -rf ./BackOfTheHouse/bin ./BackOfTheHouse/obj 2>/dev/null || true
rm -rf ./TheSandwich/bin ./TheSandwich/obj 2>/dev/null || true

# Frontend artifacts
rm -rf ./FrontOfTheHouse/dist ./FrontOfTheHouse/.angular ./FrontOfTheHouse/.vite 2>/dev/null || true

if $FULL_CLEAN; then
  warn "--full set: removing FrontOfTheHouse/node_modules (may require network to reinstall)"
  rm -rf ./FrontOfTheHouse/node_modules 2>/dev/null || true
fi

log "Clearing npm cache and ensuring frontend deps..."
if command -v npm >/dev/null 2>&1; then
  (cd FrontOfTheHouse && npm cache verify) || {
    warn "npm cache verify failed, attempting npm cache clean --force";
    (cd FrontOfTheHouse && npm cache clean --force) || warn "npm cache clean failed"
  }

  # Install dependencies (use install instead of ci to support projects without lockfile)
  (cd FrontOfTheHouse && npm install) || warn "npm install failed (check network or registry)"
else
  warn "npm not found in PATH; skipping frontend dependency steps"
fi

log "Restoring and building backend..."
if command -v dotnet >/dev/null 2>&1; then
  dotnet restore || warn "dotnet restore failed"
  dotnet build --no-restore -c Debug || warn "dotnet build failed"
else
  warn ".NET SDK not found; skipping backend build"
fi

log "Building frontend (development configuration)..."
if command -v npm >/dev/null 2>&1; then
  (cd FrontOfTheHouse && npm run build) || warn "frontend build failed"
else
  warn "npm not found; skipping frontend build"
fi

ok "Clean + build finished. You can start services with: ./dev-start-all.sh"

exit 0
