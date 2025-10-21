#!/usr/bin/env bash
set -euo pipefail

# dev-start-all.sh
# Kills backend and frontend dev servers (if running) and starts them, capturing logs.
# Improved version with better error handling, status reporting, and cleanup
# Usage: ./dev-start-all.sh [DOCKER_DB_CONNECTION]

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$ROOT_DIR"

# Colors for better output readability
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Helper functions
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

# Cleanup function for graceful exit
cleanup() {
    log_info "Cleaning up..."
    # Kill any processes we started if script is interrupted
    if [ -f "backof.pid" ]; then
        BACKEND_PID=$(cat backof.pid 2>/dev/null || true)
        if [ -n "$BACKEND_PID" ] && kill -0 "$BACKEND_PID" 2>/dev/null; then
            log_warning "Stopping backend process $BACKEND_PID"
            kill "$BACKEND_PID" 2>/dev/null || true
        fi
    fi
}

# Set trap for cleanup on script exit
trap cleanup EXIT INT TERM

# Database connection configuration
log_info "Configuring database connection..."
if [ -n "${DOCKER_DB_CONNECTION:-}" ]; then
  log_success "DOCKER_DB_CONNECTION already set in environment"
elif [ -n "${1:-}" ]; then
  export DOCKER_DB_CONNECTION="$1"
  log_success "DOCKER_DB_CONNECTION set from script argument"
elif [ -f ".env" ]; then
  # try to load DOCKER_DB_CONNECTION from .env if present (simple parse)
  val=$(grep -E '^DOCKER_DB_CONNECTION=' .env | sed -E 's/^DOCKER_DB_CONNECTION=//; s/^"//; s/"$//') || true
  if [ -n "$val" ]; then
    export DOCKER_DB_CONNECTION="$val"
    log_success "DOCKER_DB_CONNECTION loaded from .env"
  else
    log_warning "No DOCKER_DB_CONNECTION found in .env, will use SQLite"
  fi
else
  log_warning "No .env file found, will use SQLite database"
fi

# Check prerequisites
log_info "Checking prerequisites..."
if ! command -v dotnet >/dev/null 2>&1; then
    log_error ".NET SDK not found. Please install .NET SDK"
    exit 1
fi

if [ -d "FrontOfTheHouse" ]; then
    if ! command -v npm >/dev/null 2>&1; then
        log_error "npm not found. Please install Node.js and npm"
        exit 1
    fi
    
    if [ ! -f "FrontOfTheHouse/package.json" ]; then
        log_error "package.json not found in FrontOfTheHouse directory"
        exit 1
    fi
    
    if [ ! -d "FrontOfTheHouse/node_modules" ]; then
        log_warning "node_modules not found. Installing dependencies..."
        (cd FrontOfTheHouse && npm install)
    fi
else
    log_error "FrontOfTheHouse directory not found"
    exit 1
fi

# Stop existing services
log_info "Stopping any existing services..."

# More robust process stopping
stop_service() {
    local port=$1
    local service_name=$2
    local pids=$(lsof -tiTCP:$port -sTCP:LISTEN 2>/dev/null || true)
    
    if [ -n "$pids" ]; then
        log_warning "Stopping $service_name on port $port (PIDs: $pids)"
        for pid in $pids; do
            if kill -0 "$pid" 2>/dev/null; then
                kill "$pid" 2>/dev/null || true
                sleep 1
                # Force kill if still running
                if kill -0 "$pid" 2>/dev/null; then
                    log_warning "Force killing $service_name process $pid"
                    kill -9 "$pid" 2>/dev/null || true
                    sleep 1
                fi
            fi
        done
        
        # Verify the port is free
        local remaining=$(lsof -tiTCP:$port -sTCP:LISTEN 2>/dev/null || true)
        if [ -n "$remaining" ]; then
            log_error "Failed to free port $port"
            return 1
        else
            log_success "$service_name stopped successfully"
        fi
    else
        log_info "No $service_name process running on port $port"
    fi
}

stop_service 5251 "backend"
stop_service 4200 "frontend"

# Clean up old log and pid files
log_info "Cleaning up old files..."
rm -f backof.log backof.pid backofthehouse.log backofthehouse.pid front-dev.log front-dev.pid FrontOfTheHouse/front-dev.log FrontOfTheHouse/front-dev.pid

# Start backend service
log_info "Starting backend service..."
if [ -f "BackOfTheHouse.csproj" ]; then
    PROJECT_FILE="BackOfTheHouse.csproj"
elif [ -f "TheSandwich.csproj" ]; then
    PROJECT_FILE="TheSandwich.csproj"
else
    log_error "No suitable .csproj file found"
    exit 1
fi

log_info "Using project file: $PROJECT_FILE"
nohup dotnet run --project "$PROJECT_FILE" --urls="http://0.0.0.0:5251" > backofthehouse.log 2>&1 &
BACKEND_PID=$!
echo $BACKEND_PID > backofthehouse.pid
log_info "Backend started with PID $BACKEND_PID"

# Start frontend service
log_info "Starting frontend service..."
if [ -x "FrontOfTheHouse/dev-serve.sh" ]; then
    ./FrontOfTheHouse/dev-serve.sh
else
    log_warning "dev-serve.sh not executable, running directly..."
    (cd FrontOfTheHouse && {
        nohup npm run start > front-dev.log 2>&1 &
        echo $! > front-dev.pid
        log_info "Frontend started with PID $(cat front-dev.pid)"
    })
fi

# Wait for services to be available with better feedback
log_info "Waiting for services to start..."
BACKEND_READY=false
FRONTEND_READY=false
MAX_RETRIES=30

for i in $(seq 1 $MAX_RETRIES); do
    # Check backend
    if ! $BACKEND_READY && lsof -tiTCP:5251 -sTCP:LISTEN >/dev/null 2>&1; then
        BACKEND_READY=true
        log_success "Backend is ready (port 5251)"
    fi
    
    # Check frontend
    if ! $FRONTEND_READY && lsof -tiTCP:4200 -sTCP:LISTEN >/dev/null 2>&1; then
        FRONTEND_READY=true
        log_success "Frontend is ready (port 4200)"
    fi
    
    # Both ready?
    if $BACKEND_READY && $FRONTEND_READY; then
        log_success "Both services are running!"
        break
    fi
    
    # Show progress
    if [ $((i % 5)) -eq 0 ]; then
        log_info "Still waiting... (attempt $i/$MAX_RETRIES)"
        if ! $BACKEND_READY; then echo "  - Backend not ready yet"; fi
        if ! $FRONTEND_READY; then echo "  - Frontend not ready yet"; fi
    fi
    
    sleep 1
done

# Final status check
if ! $BACKEND_READY; then
    log_error "Backend failed to start after $MAX_RETRIES seconds"
    log_info "Check backofthehouse.log for details:"
    tail -n 20 backofthehouse.log 2>/dev/null || echo "No log file found"
    exit 1
fi

if ! $FRONTEND_READY; then
    log_error "Frontend failed to start after $MAX_RETRIES seconds"
    log_info "Check FrontOfTheHouse/front-dev.log for details:"
    tail -n 20 FrontOfTheHouse/front-dev.log 2>/dev/null || echo "No log file found"
    exit 1
fi

# Show service status and logs
echo ""
log_success "Development servers are running!"
echo ""
echo "📊 Service Status:"
echo "├── Backend:  http://localhost:5251 (PID: $(cat backofthehouse.pid 2>/dev/null || echo 'unknown'))"
echo "├── Frontend: http://localhost:4200 (PID: $(cat FrontOfTheHouse/front-dev.pid 2>/dev/null || echo 'unknown'))"
echo "└── Database: $([ -n "${DOCKER_DB_CONNECTION:-}" ] && echo 'SQL Server (Docker)' || echo 'SQLite (local)')"
echo ""

# Show relevant log excerpts
show_log() {
    local log_file=$1
    local service_name=$2
    local lines=${3:-15}
    
    if [ -f "$log_file" ]; then
        echo "📋 Recent $service_name logs:"
        echo "────────────────────────────────────────"
        tail -n $lines "$log_file" | sed 's/^/  /'
        echo ""
    else
        log_warning "$service_name log file not found: $log_file"
    fi
}

show_log "backofthehouse.log" "Backend" 10
show_log "FrontOfTheHouse/front-dev.log" "Frontend" 10

# Final instructions
echo "🎯 Quick Commands:"
echo "├── View backend logs:   tail -f backofthehouse.log"
echo "├── View frontend logs:  tail -f FrontOfTheHouse/front-dev.log"
echo "├── Stop all services:   pkill -f 'dotnet run' && pkill -f 'ng serve'"
echo "└── API documentation:   http://localhost:5251/swagger (if enabled)"
echo ""
log_success "Development environment ready! 🚀"

# Disable cleanup trap since we want services to keep running
trap - EXIT
