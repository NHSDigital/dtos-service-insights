# Guide: Set up local database

- [Guide: Set up local database](#guide-set-up-local-database)
  - [Overview](#overview)
  - [Prerequisites](#prerequisites)
  - [Key files](#key-files)
  - [Steps](#steps)
  - [Going forward](#going-forward)
  - [Troubleshooting](#troubleshooting)

## Overview

To run the Service Insights application on your local machine you will need to set up a local 'ServiceInsightsDB' database.

[Podman](https://podman.io) is a free and open source tool for developing, managing, and running containers. This guide uses Podman to run a containerised instance of Microsoft Azure SQL Edge.

## Prerequisites

- Install [Podman Desktop or Podman CLI](https://podman.io)
- Install [Azure Data Studio](https://learn.microsoft.com/en-us/azure-data-studio/download-azure-data-studio)
- [Set up local settings](./Set_up_local_settings.md)

## Key files

- [`create_database.sql`](../../scripts/database/create_database.sql)
- [`create_tables.sql`](../../scripts/database/create_database.sql)

## Steps

1. Ensure Podman is installed

    ```shell
    podman -v
    ```

2. Ensure that the Podman virtual machine is running

    ```shell
    podman machine start
    ```

3. Run the container

    ```shell
    podman run --name azuresqledge -e 'ACCEPT_EULA=1' -e 'MSSQL_SA_PASSWORD=YOUR_PASSWORD' -p 1433:1433 -d mcr.microsoft.com/azure-sql-edge
    ```

    Replace YOUR_PASSWORD with a password of your choice. It must comply with the SQL Server [password complexity policy](https://learn.microsoft.com/en-us/sql/relational-databases/security/password-policy?view=sql-server-ver16#password-complexity).

4. Verify that the container is running

    ```shell
    podman container list -a
    ```

5. Connect to the database server in Azure Data Studio

    - Server: localhost
    - User name: SA
    - Password: YOUR_PASSWORD

6. Create the database by running the query found in `create_database.sql` in Azure Data Studio

7. Create the database tables by running the query found in `create_tables.sql` in Azure Data Studio

8. Update the ServiceInsightsDbConnectionString environment variables found in the `local.settings.json` files, replacing YOUR_CONNECTION_STRING with

    `Server=localhost,1433;Database=ServiceInsightsDB;User Id=SA;Password=YOUR_PASSWORD;TrustServerCertificate=True`

    Remember to also replace YOUR_PASSWORD in the connection string with the password you set in step 3.

## Going forward

Going forward, to start up your Podman container you will just need to start the Podman virtual machine and start your container.

```shell
podman machine start
podman container start azuresqledge
```

## Troubleshooting

If you are having issues with running the container try the following

- Restarting the Podman virtual machine

```shell
podman machine stop
podman machine start
```

- Checking that your MSSQL_SA_PASSWORD complies with the password complexity policy

- If are using Podman Desktop, uninstall it and try Podman CLI instead
