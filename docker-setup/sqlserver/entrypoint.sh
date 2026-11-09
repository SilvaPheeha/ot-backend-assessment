#!/bin/bash
set -e

SQLCMD=/opt/mssql-tools18/bin/sqlcmd
DB_NAME="${DB_NAME:-AppDb}"


/opt/mssql/bin/sqlservr &
SQL_PID=$!


echo "Waiting for SQL Server to start..."
for i in {1..60}; do
    if $SQLCMD -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" -b -o /dev/null 2>/dev/null; then
        echo "SQL Server is up. Running init.sql..."
        $SQLCMD -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -b \
            -i /usr/src/app/init.sql -v DB_NAME="$DB_NAME"
        echo "Initialisation complete."
        break
    fi
    sleep 2
done

wait $SQL_PID
