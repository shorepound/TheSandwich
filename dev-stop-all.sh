#!/usr/bin/env bash
set -euo pipefail

# dev-stop-all.sh
# Stops all development services and cleans up pid files
# Usage: ./dev-stop-all.sh

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$ROOT_DIR"

# Colors for better output readability
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
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

log_info "Stopping all development services..."

# Function to stop service by port
stop_by_port() {
    local port=$1
    local service_name=$2
    local pids=$(lsof -tiTCP:$port -sTCP:LISTEN 2>/dev/null || true)
    
    if [ -n "$pids" ]; then
        log_info "Stopping $service_name on port $port"
        for pid in $pids; do
            if kill -0 "$pid" 2>/dev/null; then
                kill "$pid" 2>/dev/null || true
                sleep 1
                # Force kill if still running
                if kill -0 "$pid" 2>/dev/null; then
                    log_warning "Force killing $service_name process $pid"
                    kill -9 "$pid" 2>/dev/null || true
                fi
            fi
        done
        log_success "$service_name stopped"
    else
        log_info "No $service_name running on port $port"
    fi
}

# Function to stop service by PID file
stop_by_pidfile() {
    local pidfile=$1
    local service_name=$2
    
    if [ -f "$pidfile" ]; then
        local pid=$(cat "$pidfile" 2>/dev/null || true)
        if [ -n "$pid" ] && kill -0 "$pid" 2>/dev/null; then
            log_info "Stopping $service_name (PID: $pid)"
            kill "$pid" 2>/dev/null || true
            sleep 1
            if kill -0 "$pid" 2>/dev/null; then
                log_warning "Force killing $service_name process $pid"
                kill -9 "$pid" 2>/dev/null || true
            fi
            log_success "$service_name stopped"
        fi
        rm -f "$pidfile"
    fi
}

# Stop services by port (more reliable)
stop_by_port 5251 "Backend"
stop_by_port 4200 "Frontend"

# Also try to stop by PID files
stop_by_pidfile "backofthehouse.pid" "Backend"
stop_by_pidfile "backof.pid" "Backend (legacy)"
stop_by_pidfile "FrontOfTheHouse/front-dev.pid" "Frontend"
stop_by_pidfile "front-dev.pid" "Frontend (legacy)"

# Stop any remaining dotnet or ng processes related to this project
log_info "Cleaning up any remaining processes..."

# Stop dotnet processes in this directory
DOTNET_PIDS=$(pgrep -f "dotnet.*$(basename "$ROOT_DIR")" || true)
if [ -n "$DOTNET_PIDS" ]; then
    log_warning "Stopping remaining dotnet processes: $DOTNET_PIDS"
    kill $DOTNET_PIDS 2>/dev/null || true
fi

# Stop ng serve processes
NG_PIDS=$(pgrep -f "ng serve" || true)
if [ -n "$NG_PIDS" ]; then
    log_warning "Stopping ng serve processes: $NG_PIDS"
    kill $NG_PIDS 2>/dev/null || true
fi

# Final verification
sleep 2
REMAINING_5251=$(lsof -tiTCP:5251 -sTCP:LISTEN 2>/dev/null || true)
REMAINING_4200=$(lsof -tiTCP:4200 -sTCP:LISTEN 2>/dev/null || true)

if [ -n "$REMAINING_5251" ] || [ -n "$REMAINING_4200" ]; then
    log_error "Some services are still running:"
    [ -n "$REMAINING_5251" ] && echo "  - Port 5251: $REMAINING_5251"
    [ -n "$REMAINING_4200" ] && echo "  - Port 4200: $REMAINING_4200"
    echo "You may need to kill them manually or restart your terminal."
else
    log_success "All development services stopped successfully! 🛑"
fi

echo ""
log_info "To start services again, run: ./dev-start-all.sh"