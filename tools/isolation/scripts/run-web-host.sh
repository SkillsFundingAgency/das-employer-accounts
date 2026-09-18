#!/usr/bin/env bash
# Run Employer Accounts Web against local Docker deps (SQL / Redis / WireMock).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
cd "$ROOT"

DLL="$ROOT/src/SFA.DAS.EmployerAccounts.Web/bin/net10.0/SFA.DAS.EmployerAccounts.Web.dll"
if [[ ! -f "$DLL" ]]; then
  echo "Web DLL missing — building with Docker SDK..."
  "$ROOT/tools/isolation/scripts/rebuild-web-docker.sh"
fi

# Prefer a runtime that is not SIGKILL'd (see README). SDK is not required to run.
if [[ -x "$HOME/dotnet-isolation/dotnet" ]]; then
  export DOTNET_ROOT="$HOME/dotnet-isolation"
elif [[ -x /usr/local/share/dotnet/dotnet ]]; then
  export DOTNET_ROOT=/usr/local/share/dotnet
else
  export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
fi
export PATH="$DOTNET_ROOT:$PATH"

export IsolationMode=true
export ASPNETCORE_ENVIRONMENT=Development
export EnvironmentName=LOCAL
export ResourceEnvironmentName=LOCAL
export StubAuth=true
export StubEmail=isolation.employer@example.com
export StubId=11111111-1111-1111-1111-111111111111
export ASPNETCORE_URLS="${ASPNETCORE_URLS:-http://127.0.0.1:5024}"

cd "$(dirname "$DLL")"
exec "$DOTNET_ROOT/dotnet" SFA.DAS.EmployerAccounts.Web.dll
