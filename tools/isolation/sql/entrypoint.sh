#!/bin/bash
set -euo pipefail
echo "Waiting for SQL Server..."
for i in $(seq 1 60); do
  if /opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" &>/dev/null \
     || /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1" &>/dev/null; then
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

run_sql() {
  $SQLCMD -I -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" ${CFLAG:-} "$@"
}

# Prefer host-side DACPAC publish (./tools/isolation/scripts/publish-database.sh).
# If the full Database project schema is already present, only re-apply seeds.
FULL_SCHEMA=$(run_sql -d EmployerAccounts -h -1 -W -Q "SET NOCOUNT ON; SELECT CASE WHEN OBJECT_ID('employer_account.GetPayeSchemesInUse','P') IS NOT NULL AND OBJECT_ID('employer_account.CreateAccount','P') IS NOT NULL THEN 1 ELSE 0 END" 2>/dev/null | tr -d '[:space:]' || true)

if [ "$FULL_SCHEMA" = "1" ]; then
  echo "Full DACPAC schema detected — skipping bootstrap 01/04; applying seeds only."
  run_sql -i /sql/02-seed.sql
  run_sql -i /sql/03-dashboard-stubs.sql
else
  echo "No DACPAC schema yet — applying bootstrap schema (prefer publish-database.sh for full parity)."
  run_sql -i /sql/01-init-schema.sql
  run_sql -i /sql/02-seed.sql
  run_sql -i /sql/03-dashboard-stubs.sql
  run_sql -i /sql/04-paye-procs.sql
fi
echo "SQL init complete"
