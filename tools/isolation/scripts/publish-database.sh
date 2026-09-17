#!/usr/bin/env bash
# Build the EmployerAccounts DACPAC (SDK mirror of the SSDT SQL project) and publish
# it to the isolation SQL Server, then apply isolation seed data.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
DACPAC_DIR="$ROOT/tools/isolation/dacpac"
SQL_DIR="$ROOT/tools/isolation/sql"
DACPAC="$DACPAC_DIR/bin/Release/SFA.DAS.EmployerAccounts.Database.Sdk.dacpac"
SQL_CONTAINER="${ISOLATION_SQL_CONTAINER:-das-employer-accounts-isolation-verify-sqlserver-1}"
SA_PASSWORD="${MSSQL_SA_PASSWORD:-Isolation_P@ssw0rd!}"
TARGET_CS="${ISOLATION_SQL_CONNECTION:-Server=127.0.0.1,1433;Database=EmployerAccounts;User Id=sa;Password=${SA_PASSWORD};TrustServerCertificate=True;Encrypt=False}"

export PATH="${HOME}/.dotnet/tools:${DOTNET_ROOT:-$HOME/.dotnet}:$PATH"
export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"

echo "==> Building DACPAC (Docker SDK; mirrors src/SFA.DAS.EmployerAccounts.Database)"
docker run --rm \
  -v "$ROOT:/repo" \
  -w /repo/tools/isolation/dacpac \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  bash -lc 'dotnet build SFA.DAS.EmployerAccounts.Database.Sdk.sqlproj -c Release -v q'

if [[ ! -f "$DACPAC" ]]; then
  echo "DACPAC not found at $DACPAC" >&2
  exit 1
fi

if ! command -v sqlpackage >/dev/null 2>&1; then
  echo "==> Installing microsoft.sqlpackage (dotnet tool)"
  dotnet tool install -g microsoft.sqlpackage || dotnet tool update -g microsoft.sqlpackage
fi

echo "==> Recreating EmployerAccounts database on $SQL_CONTAINER"
if docker ps --format '{{.Names}}' | grep -qx "$SQL_CONTAINER"; then
  docker exec "$SQL_CONTAINER" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -Q "
IF DB_ID('EmployerAccounts') IS NOT NULL
BEGIN
  ALTER DATABASE EmployerAccounts SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
  DROP DATABASE EmployerAccounts;
END
CREATE DATABASE EmployerAccounts;
"
else
  echo "SQL container $SQL_CONTAINER not running; publish will target connection string only." >&2
fi

echo "==> Publishing DACPAC"
sqlpackage /Action:Publish \
  /SourceFile:"$DACPAC" \
  /TargetConnectionString:"$TARGET_CS" \
  /p:BlockOnPossibleDataLoss=False \
  /p:DropObjectsNotInSource=True \
  /p:AllowIncompatiblePlatform=True

echo "==> Applying isolation seed (02) + dashboard stubs (03)"
if docker ps --format '{{.Names}}' | grep -qx "$SQL_CONTAINER"; then
  for f in 02-seed.sql 03-dashboard-stubs.sql; do
    docker cp "$SQL_DIR/$f" "$SQL_CONTAINER:/tmp/$f"
    docker exec "$SQL_CONTAINER" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -d EmployerAccounts -I -i "/tmp/$f"
  done
else
  if command -v sqlcmd >/dev/null 2>&1; then
    sqlcmd -S 127.0.0.1,1433 -U sa -P "$SA_PASSWORD" -C -d EmployerAccounts -I -i "$SQL_DIR/02-seed.sql"
    sqlcmd -S 127.0.0.1,1433 -U sa -P "$SA_PASSWORD" -C -d EmployerAccounts -I -i "$SQL_DIR/03-dashboard-stubs.sql"
  else
    echo "No docker SQL container and no local sqlcmd; seeds not applied." >&2
    exit 1
  fi
fi

echo "==> Done. Schema matches Database DACPAC; seed account hash GP67XW."
