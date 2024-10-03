#!/bin/bash

# This script runs when the db-setup container is run

# Wait for db container to start
sleep 10s

ls -lah

# Set up database
/opt/mssql-tools/bin/sqlcmd -S sql-edge -U SA -P "${PASSWORD}" -i create_database.sql
/opt/mssql-tools/bin/sqlcmd -S sql-edge -U SA -P "${PASSWORD}" -d ${DB_NAME} -i create_tables.sql
/opt/mssql-tools/bin/sqlcmd -S sql-edge -U SA -P "${PASSWORD}" -d ${DB_NAME} -i insert_episode_test_data.sql

