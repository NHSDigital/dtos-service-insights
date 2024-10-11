#!/bin/bash

# Detect the OS
OS=$(uname)

# Function to start services in order
start_containers() {
  local compose_file=$1

  echo "Starting SQL Database..."
  podman compose --file "$compose_file" up -d sql-database
  podman compose --file "$compose_file" up -d database-setup

  echo "Starting Azurite..."
  podman compose --file "$compose_file" up -d azurite
  podman compose --file "$compose_file" up -d azurite-setup

  echo "Starting remaining services..."
  podman compose --file "$compose_file" up -d  --no-parallel
}

# macOS
if [ "$OS" == "Darwin" ]; then
  echo "Running on macOS..."
  start_containers "compose-mac.yaml"

# Windows (using Windows Subsystem for Linux)
elif [[ "$OS" == "Linux" && "$(uname -r)" == *"microsoft"* ]]; then
  echo "Running on Windows (WSL)..."
  start_containers "compose-win.yaml"

else
  echo "Unsupported operating system: $OS"
  exit 1
fi

echo "All containers are up and running!"
