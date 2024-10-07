# Running Containers Guide

This guide explains how to start, stop, and manage containers in your environment, with tailored instructions for both macOS and Windows users.

## Setting Up the .env File

Before starting the containers, you need to create a `.env` file based on the provided `env.template` file.

1. **Locate the Template**
   The `env.template` file is an example file that contains environment variable placeholders.

2. **Create the .env File**
   Copy the `env.template` file to a new file named `.env`:

   ```bash
   cp env.template .env
   ```

3. **Configure the .env File**
   Open the `.env` file and fill in the required values, such as `DB_NAME`, `PASSWORD`, and `DB_CONNECTION`. Example:

   ```env
   DB_NAME=ServiceInsightsDB
   PASSWORD=YourSecurePassword
   DB_CONNECTION=sql-edge  # macOS users
   #DB_CONNECTION=127.0.0.1  # Windows users
   ```

   Ensure the correct database connection details are set based on your operating system.

## Platform-Specific Setup

Due to differences in virtualization between macOS and Windows, we provide separate Docker Compose files for each platform. Follow the instructions for your operating system below.

### Starting Containers on macOS

1. **Start the Database First**
   The database service is resource-intensive, so it’s recommended to start it first:

   ```bash
   podman compose --file compose-mac.yaml up -d sql-edge

   podman compose --file compose-mac.yaml up -d db-setup

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

   podman compose --file compose.yaml up -d db-setup

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
