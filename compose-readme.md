# Running Containers Guide

This guide explains how to start, stop, restart, and manage containers in your environment, with tailored instructions for both macOS and Windows users.

## Update submodules (dotnesh-mesh-client)

1. **Update git submodules**
   The `src/Shared/dotnet-mesh-client` folder needs to be populated if you just cloned the repository.
   Run this command in the root of the repository, you only need to do this once. Afterward, the `src/Shared/dotnet-mesh-client` will be populated.

   ```bash
   git submodule update --init --recursive
   ```

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
   DB_CONNECTION=sql-database  # macOS users
   #DB_CONNECTION=127.0.0.1  # Windows users
   ```

   Ensure the correct database connection details are set based on your operating system.

## Platform-Specific Setup

Due to differences in virtualization between macOS and Windows, we provide separate Docker Compose files for each platform. Follow the instructions for your operating system below.

### Starting Containers on macOS

1. **Start the Database First**
   The database service is resource-intensive, so it’s recommended to start it first:

   ```bash
   podman compose --file compose-mac.yaml up -d sql-database
   podman compose --file compose-mac.yaml up -d database-setup
   ```

2. **Start Azurite and Azurite Setup**
   After the database is running, start Azurite and its setup service:

   ```bash
   podman compose --file compose-mac.yaml up -d azurite
   podman compose --file compose-mac.yaml up -d azurite-setup
   ```

3. **Verify Access to Azurite and SQL**
   Once both the database and Azurite services are running, you can check their availability:

   - **For Azurite (Blob Storage)**: Use **Azure Storage Explorer** to ensure access to Azurite’s blob storage.
   - **For SQL Database**: Use **Azure Data Studio** to connect to your SQL database and verify that the connection is working.

4. **Start the Remaining Services**
   Once the database and Azurite are confirmed to be running correctly, start the remaining services:

   ```bash
   podman compose --file compose-mac.yaml up -d
   ```

### Starting Containers on Windows

1. **Start the Database First**
   Like on macOS, begin by starting the database service:

   ```bash
   podman compose --file compose-win.yaml up -d sql-database
   podman compose --file compose-win.yaml up -d database-setup

   note: edit the db_setup_entrypoint.md and change to LF and save and rebuild the container
   ```

2. **Start Azurite and Azurite Setup**
   After the database, start Azurite and its setup service:

   ```bash
   podman compose --file compose-win.yaml up -d azurite
   podman compose --file compose-win.yaml up -d azurite-setup
   ```

3. **Verify Access to Azurite and SQL**

   - **For Azurite (Blob Storage)**: Use **Azure Storage Explorer** to verify access to Azurite’s blob storage.
   - **For SQL Database**: Use **Azure Data Studio** to check the connection to the SQL database.

4. **Start the Remaining Services**
   Once both the database and Azurite are confirmed to be running correctly, bring up the other services:

   ```bash
   podman compose --file compose-win.yaml up -d
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
   For example, to rebuild the `get-episode` or `sql-database` service:

  ```bash
  podman compose --file compose-mac.yaml build get-episode
  ```

  ```bash
  podman compose --file compose-mac.yaml build sql-database
  ```

Repeat the command for any other service you wish to rebuild.

## Restarting Containers

- **Restart All Containers**
  To restart all containers using your compose file:

  ```bash
  podman compose --file compose-mac.yaml restart
  ```

- **Restart All Containers (With Stop and Start)**
  Alternatively, if you want to fully stop and restart the containers:

  ```bash
  podman compose --file compose-mac.yaml down && podman compose --file compose-mac.yaml up -d
  ```
