#!/usr/bin/env bash
set -euo pipefail

# dev-start-all.sh
# Kills backend and frontend dev servers (if running) and starts them, capturing logs.
# Usage: ./dev-start-all.sh

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$ROOT_DIR"

# Allow setting DOCKER_DB_CONNECTION via: 1) existing env var, 2) first script arg, 3) .env file
if [ -n "${DOCKER_DB_CONNECTION:-}" ]; then
  echo "DOCKER_DB_CONNECTION already set in environment"
elif [ -n "${1:-}" ]; then
  export DOCKER_DB_CONNECTION="$1"
  echo "DOCKER_DB_CONNECTION set from script argument"
elif [ -f ".env" ]; then
  # try to load DOCKER_DB_CONNECTION from .env if present (simple parse)
  val=$(grep -E '^DOCKER_DB_CONNECTION=' .env | sed -E 's/^DOCKER_DB_CONNECTION=//; s/^"//; s/"$//') || true
  if [ -n "$val" ]; then
    export DOCKER_DB_CONNECTION="$val"
    echo "DOCKER_DB_CONNECTION loaded from .env"
  fi
fi

echo "Stopping backend on :5251 if running..."
B_PID=$(lsof -tiTCP:5251 -sTCP:LISTEN || true)
if [ -n "$B_PID" ]; then
  kill $B_PID || true
  sleep 1
fi

echo "Stopping frontend on :4200 if running..."
F_PID=$(lsof -tiTCP:4200 -sTCP:LISTEN || true)
if [ -n "$F_PID" ]; then
  kill $F_PID || true
  sleep 1
fi

# Start backend
echo "Starting backend (dotnet run) -> backof.log"
nohup dotnet run --project BackOfTheHouse.csproj > backof.log 2>&1 &
echo $! > backof.pid

# Start frontend helper which handles pid/log
echo "Starting frontend via FrontOfTheHouse/dev-serve.sh"
./FrontOfTheHouse/dev-serve.sh || true

# Wait for services to bind
echo "Waiting for backend (5251) and frontend (4200) to be available..."
for i in {1..20}; do
  BC=$(lsof -tiTCP:5251 -sTCP:LISTEN || true)
  FC=$(lsof -tiTCP:4200 -sTCP:LISTEN || true)
  if [ -n "$BC" ] && [ -n "$FC" ]; then
    echo "Both services are listening. backend PID=$BC frontend PID=$FC"
    break
  fi
  sleep 1
done

# Show last lines of logs
echo "--- backof.log ---"
tail -n 80 backof.log || true

echo "--- front-dev.log ---"
tail -n 80 FrontOfTheHouse/front-dev.log || true

echo "Done. Use 'tail -f backof.log' and 'tail -f FrontOfTheHouse/front-dev.log' to follow logs."
