#!/usr/bin/env bash
# Alternative to the web container: run the UI on the host against compose dependencies.
# appsettings.Isolation.json uses localhost; docker-compose overrides hostnames for the web service.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
cd "$ROOT"
export IsolationMode=true
export ASPNETCORE_ENVIRONMENT=Development
export EnvironmentName=LOCAL
export ResourceEnvironmentName=LOCAL
export StubAuth=true
export StubEmail=isolation.employer@example.com
export StubId=11111111-1111-1111-1111-111111111111
export ASPNETCORE_URLS='http://localhost:5024'
dotnet run --project src/SFA.DAS.EmployerAccounts.Web/SFA.DAS.EmployerAccounts.Web.csproj -c Debug --no-launch-profile
