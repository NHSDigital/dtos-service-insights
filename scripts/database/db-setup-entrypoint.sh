#!/bin/bash

# This script runs when the db-setup container is run

# Log the variables to verify they are set
echo "DB_CONNECTION: ${DB_CONNECTION}"
# echo "PASSWORD: ${PASSWORD}"
echo "DB_NAME: ${DB_NAME}"

# Check for empty variables
if [[ -z "${DB_CONNECTION}" || -z "${PASSWORD}" || -z "${DB_NAME}" ]]; then
    echo "Error: One or more required variables are not set."
    exit 1
fi

# Wait for db container to start
echo "Waiting for the database to start..."
for i in {1..30}; do
    if /opt/mssql-tools/bin/sqlcmd -S "${DB_CONNECTION}" -U SA -P "${PASSWORD}" -Q "SELECT 1;" &> /dev/null; then
        echo "Database is up!"
        break
    fi
    echo "Waiting for database... Attempt $i/30"
    sleep 1
done

# Check if the loop timed out
if [ $i -eq 30 ]; then
    echo "Error: Database did not start in time."
    exit 1
fi

# Set up database
echo "Setting up the database..."
/opt/mssql-tools/bin/sqlcmd -S "${DB_CONNECTION}" -U SA -P "${PASSWORD}" -i create_database.sql || { echo "Failed to create database"; exit 1; }
/opt/mssql-tools/bin/sqlcmd -S "${DB_CONNECTION}" -U SA -P "${PASSWORD}" -d "${DB_NAME}" -i drop_tables.sql || { echo "Failed to drop tables"; exit 1; }
/opt/mssql-tools/bin/sqlcmd -S "${DB_CONNECTION}" -U SA -P "${PASSWORD}" -d "${DB_NAME}" -i create_tables.sql || { echo "Failed to create tables"; exit 1; }
/opt/mssql-tools/bin/sqlcmd -S "${DB_CONNECTION}" -U SA -P "${PASSWORD}" -d "${DB_NAME}" -i insert_test_data.sql || { echo "Failed to insert test data"; exit 1; }

echo "Database setup complete."
