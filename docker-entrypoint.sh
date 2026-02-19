#!/bin/bash
set -e

# wait for MySQL to be ready (host/port come from environment or default to 'db:3306')
DB_HOST=${DB_HOST:-db}
DB_PORT=${DB_PORT:-3306}
TIMEOUT=${DB_WAIT_TIMEOUT:-60}

echo "Waiting for database ${DB_HOST}:${DB_PORT} (timeout=${TIMEOUT}s) ..."
count=0
while ! (</dev/tcp/${DB_HOST}/${DB_PORT}) >/dev/null 2>&1; do
  count=$((count+1))
  if [ "$count" -ge "$TIMEOUT" ]; then
    echo "Timed out waiting for ${DB_HOST}:${DB_PORT} after ${TIMEOUT}s"
    exit 1
  fi
  sleep 1
done

echo "Database is reachable. Starting the application..."
# Program.cs will run db.Database.Migrate() on startup
exec dotnet EmployeeDemo.API.dll