#!/bin/bash

# CI/CD Build Script for AgentUI React Application
# This script builds and deploys the React application using Docker

set -e  # Exit on any error

# Configuration
APP_NAME="agent-ui"
CONTAINER_NAME="agent-ui-container"
IMAGE_NAME="agent-ui:latest"
PORT=3000

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

# Function to cleanup existing containers
cleanup() {
    log "Cleaning up existing containers and images..."
    
    # Stop and remove existing container if it exists
    if docker ps -a --format 'table {{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        warning "Stopping existing container: ${CONTAINER_NAME}"
        docker stop ${CONTAINER_NAME} || true
        docker rm ${CONTAINER_NAME} || true
    fi
    
    # Remove existing image if it exists
    if docker images --format 'table {{.Repository}}:{{.Tag}}' | grep -q "^${IMAGE_NAME}$"; then
        warning "Removing existing image: ${IMAGE_NAME}"
        docker rmi ${IMAGE_NAME} || true
    fi
    
    success "Cleanup completed"
}

# Function to build Docker image
build_image() {
    log "Building Docker image: ${IMAGE_NAME}"
    
    # Change to the React app directory
    cd /home/ubuntu/ai-agent/AgentUI/agent-chat
    
    # Build the Docker image
    docker build -t ${IMAGE_NAME} .
    
    if [ $? -eq 0 ]; then
        success "Docker image built successfully: ${IMAGE_NAME}"
    else
        error "Failed to build Docker image"
        exit 1
    fi
}

# Function to run Docker container
run_container() {
    log "Starting Docker container: ${CONTAINER_NAME}"
    
    # Run the container
    docker run -d \
        --name ${CONTAINER_NAME} \
        -p ${PORT}:80 \
        --restart unless-stopped \
        ${IMAGE_NAME}
    
    if [ $? -eq 0 ]; then
        success "Container started successfully: ${CONTAINER_NAME}"
        log "Application is running on http://localhost:${PORT}"
    else
        error "Failed to start container"
        exit 1
    fi
}

# Function to check container health
check_health() {
    log "Checking container health..."
    
    # Wait a moment for the container to start
    sleep 5
    
    # Check if container is running
    if docker ps --format 'table {{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        success "Container is running successfully"
        
        # Test HTTP response
        if curl -f -s http://localhost:${PORT} > /dev/null; then
            success "Application is responding to HTTP requests"
        else
            warning "Application may not be fully ready yet"
        fi
    else
        error "Container is not running"
        docker logs ${CONTAINER_NAME}
        exit 1
    fi
}

# Function to show deployment info
show_info() {
    log "Deployment Information:"
    echo "=========================="
    echo "Application: ${APP_NAME}"
    echo "Container: ${CONTAINER_NAME}"
    echo "Image: ${IMAGE_NAME}"
    echo "Port: ${PORT}"
    echo "URL: http://localhost:${PORT}"
    echo "=========================="
    
    log "Container status:"
    docker ps --filter "name=${CONTAINER_NAME}" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
}

# Main execution
main() {
    log "Starting CI/CD deployment for ${APP_NAME}"
    
    # Check if Docker is running
    if ! docker info > /dev/null 2>&1; then
        error "Docker is not running. Please start Docker and try again."
        exit 1
    fi
    
    # Execute deployment steps
    cleanup
    build_image
    run_container
    check_health
    show_info
    
    success "Deployment completed successfully!"
    log "You can access the application at: http://localhost:${PORT}"
}

# Execute main function
main "$@"

