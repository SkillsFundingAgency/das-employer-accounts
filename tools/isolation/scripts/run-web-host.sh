#!/usr/bin/env bash
# Alternative to the web container: run the UI on the host against compose dependencies.
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
# Host -> published container ports
export SFA__DAS__EmployerAccounts__DatabaseConnectionString='Server=localhost,1433;Database=EmployerAccounts;User Id=sa;Password=Isolation_P@ssw0rd!;TrustServerCertificate=True;Encrypt=False'
export SFA__DAS__EmployerAccounts__RedisConnectionString='localhost:6379'
export SFA__DAS__EmployerAccounts__EmployerAccountsOuterApiConfiguration__BaseUrl='http://localhost:8080/'
export SFA__DAS__EmployerAccounts__EmployerAccountsOuterApiConfiguration__Key='isolation-fake-subscription-key'
export SFA__DAS__EmployerAccounts__ContentApi__ApiBaseUrl='http://localhost:8080/'
export SFA__DAS__EmployerAccounts__ContentApi__IdentifierUri=''
export SFA__DAS__EmployerAccounts__AccountApi__ApiBaseUrl='http://localhost:8080/account-api/'
dotnet run --project src/SFA.DAS.EmployerAccounts.Web/SFA.DAS.EmployerAccounts.Web.csproj -c Debug --no-launch-profile
