#!/bin/bash

# Goalbound Family - Complete Application Startup Script
# Starts .NET backend (with Azure Computer Vision OCR) and React frontend
# NOTE: Python OCR service no longer needed - migrated to Azure Computer Vision

set -e

# Color codes for better output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Ports used by the application
BACKEND_PORT=5001
FRONTEND_PORT=5173

# PID tracking
BACKEND_PID=""
FRONTEND_PID=""

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  Goalbound Family - Application Start ${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

# Function to print colored messages
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to kill process on a specific port
kill_port() {
    local port=$1
    local pid=$(lsof -ti:$port 2>/dev/null)

    if [ ! -z "$pid" ]; then
        log_warn "Port $port is in use by PID $pid. Killing process..."
        kill -9 $pid 2>/dev/null || true
        sleep 1
        log_info "Port $port is now free"
    fi
}

# Function to cleanup all processes on exit
cleanup() {
    echo ""
    log_warn "Shutting down all services..."

    # Kill .NET backend
    if [ ! -z "$BACKEND_PID" ]; then
        log_info "Stopping .NET backend (PID: $BACKEND_PID)..."
        kill -TERM $BACKEND_PID 2>/dev/null || true
        wait $BACKEND_PID 2>/dev/null || true
    fi

    # Kill frontend
    if [ ! -z "$FRONTEND_PID" ]; then
        log_info "Stopping React frontend (PID: $FRONTEND_PID)..."
        kill -TERM $FRONTEND_PID 2>/dev/null || true
        wait $FRONTEND_PID 2>/dev/null || true
    fi

    # Extra cleanup: kill any remaining processes on our ports
    kill_port $BACKEND_PORT
    kill_port $FRONTEND_PORT

    log_info "Cleanup complete!"
    echo ""
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}  All services stopped. Goodbye! 👋    ${NC}"
    echo -e "${BLUE}========================================${NC}"
    exit 0
}

# Trap CTRL+C (SIGINT) and SIGTERM
trap cleanup SIGINT SIGTERM

# Step 1: Clean up ports
log_info "Checking for processes on required ports..."
kill_port $BACKEND_PORT
kill_port $FRONTEND_PORT
echo ""

# Step 2: Start .NET Backend (with Azure Computer Vision OCR)
log_info "Starting .NET backend on port $BACKEND_PORT..."
cd backend

# Create logs directory if it doesn't exist
mkdir -p ../logs

dotnet watch run > ../logs/backend.log 2>&1 &
BACKEND_PID=$!

cd ..
log_info ".NET backend started (PID: $BACKEND_PID)"

# Wait for backend to be ready
log_info "Waiting for .NET backend to initialize..."
sleep 5

# Check if backend is responding
for i in {1..10}; do
    if curl -s http://localhost:$BACKEND_PORT > /dev/null 2>&1; then
        log_info ".NET backend is ready! ✓"
        break
    fi

    if [ $i -eq 10 ]; then
        log_error ".NET backend failed to start. Check logs/backend.log"
        cleanup
    fi

    echo -n "."
    sleep 2
done
echo ""

# Step 3: Start React Frontend
log_info "Starting React frontend on port $FRONTEND_PORT..."
cd frontend

# Check if node_modules exists
if [ ! -d "node_modules" ]; then
    log_warn "Node modules not installed. Installing..."
    npm install
fi

npm run dev > ../logs/frontend.log 2>&1 &
FRONTEND_PID=$!

cd ..
log_info "React frontend started (PID: $FRONTEND_PID)"

# Wait for frontend to be ready
log_info "Waiting for React frontend to initialize..."
sleep 3

for i in {1..10}; do
    if curl -s http://localhost:$FRONTEND_PORT > /dev/null 2>&1; then
        log_info "React frontend is ready! ✓"
        break
    fi

    if [ $i -eq 10 ]; then
        log_error "React frontend failed to start. Check logs/frontend.log"
        cleanup
    fi

    echo -n "."
    sleep 2
done
echo ""

# All services started successfully
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  ✓ All services are running!          ${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${BLUE}Service URLs:${NC}"
echo -e "  • .NET Backend: ${GREEN}http://localhost:$BACKEND_PORT${NC}"
echo -e "  • React Frontend: ${GREEN}http://localhost:$FRONTEND_PORT${NC}"
echo ""
echo -e "${BLUE}OCR Engine:${NC}"
echo -e "  • Azure Computer Vision (95-99% accuracy)"
echo ""
echo -e "${BLUE}Logs:${NC}"
echo -e "  • Backend:     ${YELLOW}logs/backend.log${NC}"
echo -e "  • Frontend:    ${YELLOW}logs/frontend.log${NC}"
echo ""
echo -e "${YELLOW}Press CTRL+C to stop all services${NC}"
echo ""

# Keep script running and wait for CTRL+C
wait
