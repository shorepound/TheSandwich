#!/usr/bin/env bash
set -euo pipefail

# dev-status.sh
# Shows the status of development services
# Usage: ./dev-status.sh

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$ROOT_DIR"

# Colors for better output readability
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

check_service() {
    local port=$1
    local service_name=$2
    local url=$3
    
    local pids=$(lsof -tiTCP:$port -sTCP:LISTEN 2>/dev/null || true)
    
    if [ -n "$pids" ]; then
        echo -e "├── ${GREEN}✓${NC} $service_name (PID: $pids) - $url"
        return 0
    else
        echo -e "├── ${RED}✗${NC} $service_name - Not running"
        return 1
    fi
}

check_health() {
    local url=$1
    local service_name=$2
    
    if command -v curl >/dev/null 2>&1; then
        if curl -s --max-time 3 "$url" >/dev/null 2>&1; then
            echo -e "│   ${GREEN}→${NC} Health check: OK"
        else
            echo -e "│   ${YELLOW}→${NC} Health check: Failed (may still be starting)"
        fi
    fi
}

echo ""
echo "🔍 Development Services Status"
echo "═══════════════════════════════════════════"

BACKEND_RUNNING=false
FRONTEND_RUNNING=false

# Check backend
if check_service 5251 "Backend API" "http://localhost:5251"; then
    BACKEND_RUNNING=true
    check_health "http://localhost:5251/api/sandwiches" "Backend"
fi

# Check frontend  
if check_service 4200 "Frontend App" "http://localhost:4200"; then
    FRONTEND_RUNNING=true
    check_health "http://localhost:4200" "Frontend"
fi

echo "└── Overall Status: $(if $BACKEND_RUNNING && $FRONTEND_RUNNING; then echo -e "${GREEN}All Services Running${NC}"; elif $BACKEND_RUNNING || $FRONTEND_RUNNING; then echo -e "${YELLOW}Partial${NC}"; else echo -e "${RED}All Services Stopped${NC}"; fi)"

echo ""
echo "📊 Environment Information"
echo "═══════════════════════════════════════════"
echo "├── Database: $([ -n "${DOCKER_DB_CONNECTION:-}" ] && echo 'SQL Server (Docker)' || echo 'SQLite (local)')"
echo "├── .NET Version: $(dotnet --version 2>/dev/null || echo 'Not found')"
echo "├── Node Version: $(node --version 2>/dev/null || echo 'Not found')"
echo "└── npm Version: $(npm --version 2>/dev/null || echo 'Not found')"

# Show PID files status
echo ""
echo "📁 PID Files"
echo "═══════════════════════════════════════════"
for pidfile in backofthehouse.pid backof.pid FrontOfTheHouse/front-dev.pid front-dev.pid; do
    if [ -f "$pidfile" ]; then
        pid=$(cat "$pidfile" 2>/dev/null || echo "invalid")
        if kill -0 "$pid" 2>/dev/null; then
            echo -e "├── ${GREEN}✓${NC} $pidfile (PID: $pid - running)"
        else
            echo -e "├── ${YELLOW}⚠${NC} $pidfile (PID: $pid - stale)"
        fi
    else
        echo -e "├── ${GRAY}○${NC} $pidfile (not found)"
    fi
done

# Show log files status
echo ""
echo "📋 Log Files"
echo "═══════════════════════════════════════════"
for logfile in backofthehouse.log backof.log FrontOfTheHouse/front-dev.log front-dev.log; do
    if [ -f "$logfile" ]; then
        size=$(ls -lh "$logfile" | awk '{print $5}')
        modified=$(ls -l "$logfile" | awk '{print $6, $7, $8}')
        echo -e "├── ${GREEN}✓${NC} $logfile ($size, modified: $modified)"
    else
        echo -e "├── ${GRAY}○${NC} $logfile (not found)"
    fi
done

echo ""
echo "🎯 Quick Commands"
echo "═══════════════════════════════════════════"
echo "├── Start services:  ./dev-start-all.sh"
echo "├── Stop services:   ./dev-stop-all.sh"
echo "├── View backend:    tail -f backofthehouse.log"
echo "├── View frontend:   tail -f FrontOfTheHouse/front-dev.log"
echo "└── Check status:    ./dev-status.sh"
echo ""