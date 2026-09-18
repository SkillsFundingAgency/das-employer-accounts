#!/usr/bin/env bash
# Publish a built EmployerAccounts .dacpac to the isolation SQL Server.
# Build the DACPAC on Windows/VS or take it from CI — the classic SSDT sqlproj
# does not build on macOS.
set -euo pipefail
DACPAC="${1:-}"
if [[ -z "$DACPAC" || ! -f "$DACPAC" ]]; then
  echo "Usage: $0 /path/to/SFA.DAS.EmployerAccounts.Database.dacpac" >&2
  exit 1
fi
SQLPACKAGE="${SQLPACKAGE:-$(command -v sqlpackage || true)}"
if [[ -z "$SQLPACKAGE" ]]; then
  echo "sqlpackage not found. Install: dotnet tool install -g microsoft.sqlpackage" >&2
  exit 1
fi
CONN='Server=localhost,1433;Database=EmployerAccounts;User Id=sa;Password=Isolation_P@ssw0rd!;TrustServerCertificate=True;Encrypt=False'
"$SQLPACKAGE" /Action:Publish /SourceFile:"$DACPAC" /TargetConnectionString:"$CONN" /p:BlockOnPossibleDataLoss=false
echo "Published $DACPAC to EmployerAccounts"
