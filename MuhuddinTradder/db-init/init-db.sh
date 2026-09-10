#!/bin/bash

echo "Waiting for SQL Server to start..."

until /opt/mssql-tools18/bin/sqlcmd \
  -S db \
  -U sa \
  -P "$MSSQL_SA_PASSWORD" \
  -C \
  -Q "SELECT 1" > /dev/null 2>&1
do
  sleep 2
done

echo "SQL Server is ready."

/opt/mssql-tools18/bin/sqlcmd \
  -S db \
  -U sa \
  -P "$MSSQL_SA_PASSWORD" \
  -C \
  -i /scripts/mtdb-docker.sql

echo "Database initialization completed."
