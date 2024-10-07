# Running Containers Guide

This guide explains how to start, stop, and manage containers in your environment, with tailored instructions for both macOS and Windows users.

## Platform-Specific Setup

Due to differences in virtualization between macOS and Windows, we provide separate Docker Compose files for each platform. Follow the instructions for your operating system below.

### Starting Containers on macOS

1. **Start the Database First**
   The database service is resource-intensive, so it’s recommended to start it first:

   ```bash
   podman compose --file compose-mac.yaml up -d sql-edge
   ```

2. **Start the Remaining Services**
   Once the database is running, you can start the remaining services:

   ```bash
   podman compose --file compose-mac.yaml up -d
   ```

### Starting Containers on Windows

1. **Start the Database First**
   Like on macOS, begin by starting the database service:

   ```bash
   podman compose --file compose.yaml up -d sql-edge
   ```

2. **Start the Remaining Services**
   Then, bring up the other services:

   ```bash
   podman compose --file compose.yaml up -d
   ```

## Stopping Containers

- **Stop All Containers**
  To stop all running containers:

  ```bash
  podman compose down
  ```

- **Stop a Specific Container**
  To stop a particular service, specify the service name:

  ```bash
  podman compose down get-episode
  ```

## Rebuilding Containers

If you have made changes to the code and need to rebuild the container image, use the following commands:

- **Rebuild a Specific Service**
  For example, to rebuild the `get-episode` or `sql-edge` service:

  ```bash
  podman compose --file compose-mac.yaml build get-episode
  ```

  ```bash
  podman compose --file compose-mac.yaml build sql-edge
  ```

Repeat the command for any other service you wish to rebuild.
