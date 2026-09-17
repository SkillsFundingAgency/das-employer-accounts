#!/bin/bash
set -euo pipefail
echo "Waiting for SQL Server..."
for i in $(seq 1 60); do
  if /opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" &>/dev/null      || /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1" &>/dev/null; then
    echo "SQL is up"
    break
  fi
  sleep 2
done

SQLCMD=$(command -v sqlcmd || true)
if [ -z "${SQLCMD}" ]; then
  if [ -x /opt/mssql-tools18/bin/sqlcmd ]; then SQLCMD=/opt/mssql-tools18/bin/sqlcmd; CFLAG=-C;
  elif [ -x /opt/mssql-tools/bin/sqlcmd ]; then SQLCMD=/opt/mssql-tools/bin/sqlcmd; CFLAG=;
  else echo "sqlcmd not found"; exit 1; fi
else
  CFLAG=-C
fi

echo "Applying isolation seed (schema must already be published via Database DACPAC)..."
$SQLCMD -I -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" ${CFLAG:-} -i /sql/02-seed.sql
echo "SQL seed complete"
