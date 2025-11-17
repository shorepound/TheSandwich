#!/usr/bin/env bash
set -euo pipefail

# scripts/setup-local.sh
# Lightweight helper to prepare a local dev environment.

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT_DIR"

echo "Installing frontend dependencies (this may take a minute)..."
if [ -d "FrontOfTheHouse" ]; then
  (cd FrontOfTheHouse && npm install)
else
  echo "Warning: FrontOfTheHouse directory not found; skipping npm install"
fi

echo "Ensuring Data directory exists for SQLite fallback..."
mkdir -p Data

echo "Local setup complete. Run './dev-start-all.sh' from the repo root to start the dev servers."
