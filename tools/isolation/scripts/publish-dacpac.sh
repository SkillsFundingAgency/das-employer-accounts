#!/usr/bin/env bash
# Build the database DACPAC (works on macOS/Linux/Windows) and publish to isolation SQL.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
cd "$ROOT"

SDK_PROJ="src/SFA.DAS.EmployerAccounts.Database/SFA.DAS.EmployerAccounts.Database.Sdk.sqlproj"
DACPAC="src/SFA.DAS.EmployerAccounts.Database/bin/Release/SFA.DAS.EmployerAccounts.Database.Sdk.dacpac"

echo "Building DACPAC..."
dotnet build "$SDK_PROJ" -c Release --nologo

if [[ ! -f "$DACPAC" ]]; then
  echo "DACPAC not found at $DACPAC" >&2
  exit 1
fi

SQLPACKAGE="${SQLPACKAGE:-$(command -v sqlpackage || true)}"
if [[ -z "$SQLPACKAGE" ]]; then
  echo "sqlpackage not found. Install with: dotnet tool install -g microsoft.sqlpackage" >&2
  exit 1
fi

CONN='Server=localhost,1433;Database=EmployerAccounts;User Id=sa;Password=Isolation_P@ssw0rd!;TrustServerCertificate=True;Encrypt=False'
echo "Publishing to isolation SQL..."
"$SQLPACKAGE" /Action:Publish /SourceFile:"$DACPAC" /TargetConnectionString:"$CONN" /p:BlockOnPossibleDataLoss=false
echo "Done."
