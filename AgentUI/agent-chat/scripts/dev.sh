#!/bin/bash

# CI/CD Development Script for AgentUI React Application
# This script sets up the development environment and runs the app locally

set -e  # Exit on any error

# Configuration
APP_NAME="agent-ui"
PORT=5173

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging function
log() {
    echo -e "${BLUE}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Function to check prerequisites
check_prerequisites() {
    log "Checking prerequisites..."
    
    # Check Node.js
    if ! command -v node &> /dev/null; then
        error "Node.js is not installed"
        exit 1
    fi
    
    # Check npm
    if ! command -v npm &> /dev/null; then
        error "npm is not installed"
        exit 1
    fi
    
    # Check pnpm
    if ! command -v pnpm &> /dev/null; then
        warning "pnpm is not installed, installing..."
        npm install -g pnpm
    fi
    
    success "Prerequisites check completed"
}

# Function to install dependencies
install_dependencies() {
    log "Installing dependencies..."
    
    # Change to the React app directory
    cd /home/ubuntu/ai-agent/AgentUI/agent-chat
    
    # Install dependencies using pnpm
    pnpm install
    
    if [ $? -eq 0 ]; then
        success "Dependencies installed successfully"
    else
        error "Failed to install dependencies"
        exit 1
    fi
}

# Function to run linting
run_lint() {
    log "Running ESLint..."
    
    # Run ESLint
    pnpm run lint || warning "Linting completed with warnings"
    
    success "Linting completed"
}

# Function to run development server
run_dev_server() {
    log "Starting development server..."
    
    success "Development server will start on http://localhost:${PORT}"
    log "Press Ctrl+C to stop the server"
    
    # Start the development server
    pnpm run dev
}

# Function to build for production
build_production() {
    log "Building for production..."
    
    # Build the application
    pnpm run build
    
    if [ $? -eq 0 ]; then
        success "Production build completed successfully"
        log "Build files are in the 'dist' directory"
    else
        error "Production build failed"
        exit 1
    fi
}

# Function to preview production build
preview_production() {
    log "Starting production preview server..."
    
    # Preview the production build
    pnpm run preview
}

# Function to show help
show_help() {
    echo "Usage: $0 [COMMAND]"
    echo ""
    echo "Commands:"
    echo "  dev       Start development server (default)"
    echo "  build     Build for production"
    echo "  preview   Preview production build"
    echo "  lint      Run ESLint"
    echo "  help      Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 dev      # Start development server"
    echo "  $0 build    # Build for production"
    echo "  $0 preview  # Preview production build"
}

# Main execution
main() {
    local command=${1:-dev}
    
    log "Starting CI/CD development workflow for ${APP_NAME}"
    
    case $command in
        "dev")
            check_prerequisites
            install_dependencies
            run_lint
            run_dev_server
            ;;
        "build")
            check_prerequisites
            install_dependencies
            run_lint
            build_production
            ;;
        "preview")
            check_prerequisites
            install_dependencies
            build_production
            preview_production
            ;;
        "lint")
            check_prerequisites
            install_dependencies
            run_lint
            ;;
        "help")
            show_help
            ;;
        *)
            error "Unknown command: $command"
            show_help
            exit 1
            ;;
    esac
}

# Execute main function
main "$@"

