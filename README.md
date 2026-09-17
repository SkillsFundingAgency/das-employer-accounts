# Digital Apprenticeships Service

## Employer Apprenticeship Service

|               |               |
| ------------- | ------------- |
|![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png)|Employer Apprenticeship Service|
| Build | ![Build Status](https://sfa-gov-uk.visualstudio.com/_apis/public/build/definitions/c39e0c0b-7aff-4606-b160-3566f3bbce23/101/badge) |
| Web  | https://manage-apprenticeships.service.gov.uk/  |

## Account Api

|               |               |
| ------------- | ------------- |
|![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png)| Account API |
| Client  | [![NuGet Badge](https://buildstats.info/nuget/SFA.DAS.Account.Api.Client)](https://www.nuget.org/packages/SFA.DAS.Account.Api.Client)  |


### Developer Setup

#### Requirements

1. Install [Visual Studio] with these workloads:
    - ASP.NET and web development
    - Azure development
    - .NET desktop development
2. Install [SQL Server Management Studio]
3. Install [Azure Storage Explorer]
4. Install [CosmosDB Emulator]
5. Administator Access

#### Setup

##### Install SSL certificates for HTTPS on IIS express

- Request a SAS connection to the certs blob storage from from Dev Ops
- Download DasIDPCert.pfx to the das-employerapprenticeshipsservice/src folder
- Open PowerShell as an administrator
- Run src\DevInstall.ps1
- DO NOT COMMIT THE UPDATED CERTIFICATE TO GIT


##### Open the solution

- Open Visual Studio as an administrator
- Open the solution
- Set the following as the startup project:
	- SFA.DAS.EmployerAccounts.Web
- Running the solution will launch the site in your browser

##### Publish the databases

Repeat these steps for:

1. SFA.DAS.EAS.Employer_Account.Database

Note: If you have an existing database you may need to drop it first, to avoid the script aborting after an error on truncating data.

Steps:

* Right click on the db project in the solution explorer
* Click on publish menu item
* Click the edit button

![Click the edit button](/docs/img/db1.PNG)

* Select Local > ProjectsV13

![Select Local > ProjectsV13](/docs/img/db2.PNG)

* Add the project name in again as the Database name (i.e. SFA.DAS.EAS.Employer_Account.Database)
* Click publish

![Select Local > ProjectsV13](/docs/img/db3.PNG)

##### Add configuration to Azure Storage Emulator

The configuration is loaded from azure table storage.

* Run the Azure Storage Emulator
* Clone the [das-employer-config](https://github.com/SkillsFundingAgency/das-employer-config) repository
* Clone the [das-employer-config-updater](https://github.com/SkillsFundingAgency/das-employer-config-updater) repository
* Run the das-employer-config-updater console application and follow the instructions to import the config from the das-employer-config directory

> The two repos above are private. If the links appear to be dead make sure you are logged into github with an account that has access to these (i.e. that you are part of the Skills Funding Agency Team organization).

##### Create an Azure table storage account

- Create an [Azure] account and set up a table storage
- Create a table called Configuration and copy across these 4 rows from the employer config setup above changing the PartitionKey to "Development" from "LOCAL":
* SFA.DAS.EmployerApprenticeshipsService._1.0
* SFA.DAS.EmployerAccounts_1.0
* SFA.DAS.EmployerApprenticeshipsService.FeaturesV2_1.0
* SFA.DAS.EmployerApprenticeshipsService.MultiVariantTesting_1.0

##### Set up enivronment variables

- Create two new environment variables:
* ASPNETCORE_ENVIRONMENT set to Development
* APPSETTING_ConfigurationStorageConnectionString set to the connection string of the Azure table storage created above
 
Using `"Action": "*"` can also be used to disable all actions on the controller.

[Azure Storage Explorer]: http://storageexplorer.com/
[Choclatey]: https://chocolatey.org
[Docker]: https://www.docker.com
[Elastic Search]: https://www.elastic.co/products/elasticsearch
[SFA.DAS.Activities]: https://github.com/SkillsFundingAgency/das-activities/blob/master/README.md
[SQL Server Management Studio]: https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms
[Visual Studio]: https://www.visualstudio.com
[CosmosDB Emulator]: https://aka.ms/cosmosdb-emulator
[Azure]: https://azure.microsoft.com/en-us/

## SonarCloud Analysis

SonarCloud analysis can be performed using a docker container which can be built from the included dockerfile.
An example of the docker run command to analyse the code base can be found below. 

For this docker container to be successfully created you will need:
* a user on SonarCloud.io with permission to run analysis
* a SonarQube.Analysis.xml file in the root of the git repository.

This file takes the format:

```xml
<SonarQubeAnalysisProperties  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns="http://www.sonarsource.com/msbuild/integration/2015/1">
<Property Name="sonar.host.url">https://sonarcloud.io</Property>
<Property Name="sonar.login">[Your SonarCloud user token]</Property>
</SonarQubeAnalysisProperties>
```

### Example:

_docker run [OPTIONS] IMAGE COMMAND_ 

[Docker run documentation](https://docs.docker.com/engine/reference/commandline/run/)

```docker run --rm -v c:/projects/das-employerapprenticeshipsservice:c:/projects/das-employerapprenticeshipsservice -w c:/projects/das-employerapprenticeshipsservice 3d9151a444b2 powershell -F c:/projects/das-employerapprenticeshipsservice/analyse.ps1```

#### Options:

|Option|Description|
|---|---|
|--rm| Remove any existing containers for this image
|-v| Bind the current directory of the host to the given directory in the container ($PWD may be different on your platform). This should be the folder where the code to be analysed is
|-w| Set the working directory

#### Command:

Execute the analyse.ps1 PowerShell script

## See Also
* [Integration Tests](docs/IntegrationTesting.md "Integration Testing")
* [Authorization Pipeline](docs/AuthorizationPipeline.md "Authorization Pipeline")
* [Running Jobs](docs/Jobs/RunningJobs.md "Running Jobs")

## Running in complete isolation

Run the Employer Accounts **Web UI** locally without Azure App Configuration, Key Vault, das-employer-config, real DfE Sign-in, Azure Service Bus, or live Outer APIs.

### What this stack provides

| Component | Role |
|-----------|------|
| `web` | `SFA.DAS.EmployerAccounts.Web` (Debug) with `StubAuth` |
| `sqlserver` + `sql-init` | SQL Server 2022 + bootstrap schema/seed (`GP67XW` account) |
| `redis` | Cache / Gov login session string target |
| `wiremock` | Outer API, Content API, and Account API stubs |

NServiceBus uses **LearningTransport** when `EnvironmentName=LOCAL` (already wired in code). All config values under isolation are **FAKE**.

### Prerequisites

1. Docker Desktop (Compose v2)
2. .NET SDK on the host if using the host-run script (`tools/isolation/scripts/run-web-host.sh`) — `dotnet restore` uses **public** NuGet feeds for this service (no private Azure Artifacts PAT required)

### Clean README verification (optional)

To prove a fresh clone works without touching your day-to-day working copy, clone into a **separate** directory (for example `/tmp/das-employer-accounts-isolation-verify`), check out `APPMAN-1150`, and follow the steps below there. Do **not** delete your existing local clone.

### Copy-paste: full compose (UI in container)

```bash
cd /path/to/das-employer-accounts
git checkout APPMAN-1150

# Preferred: dependencies in Docker, UI on the host
docker compose up -d sqlserver redis wiremock
docker compose up sql-init
./tools/isolation/scripts/run-web-host.sh

# Optional: full stack in Compose (UI container)
# docker compose up --build
```

Open **http://localhost:5024/**  
(macOS: AirPlay often owns port 5000 — isolation binds **5024** on purpose.)

Stub login: **http://localhost:5024/service/SignIn-Stub**  
(pre-filled with `isolation.employer@example.com` / stub id `11111111-1111-1111-1111-111111111111`)

WireMock admin: http://localhost:8080/__admin/

Reset DB volume if schema init was partial:

```bash
docker compose down -v
docker compose up --build
```

### Copy-paste: dependencies in Docker, Web on host

Use this when the web image cannot restore private packages inside Docker but your host already restores:

```bash
cd /path/to/das-employer-accounts
git checkout APPMAN-1150
docker compose up sqlserver sql-init redis wiremock
./tools/isolation/scripts/run-web-host.sh
```

### Auth (stub)

- `StubAuth=true` and DfE Sign-in stub routes are compiled in **Debug** (isolation Dockerfile builds Debug).
- Do **not** use real GovUk OIDC client secrets; `IdentifierUri` for Content/Account APIs is empty so no Azure AD token is requested.

### Developer-supplied stubs / known blockers

| Blocker | What you must supply |
|---------|----------------------|
| NuGet restore | Public feeds are enough for this service; private Azure Artifacts is **not** required for isolation |
| Full DACPAC | `tools/isolation/sql/01-init-schema.sql` is a **bootstrap** of core tables only. For journeys that hit missing procs/views, publish `src/SFA.DAS.EmployerAccounts.Database` to `EmployerAccounts` and re-run `02-seed.sql` |
| Windows-only Sonar `Dockerfile` | Ignored for isolation; use `tools/isolation/Dockerfile.web` |
| das-employer-config / Azurite config table | Skipped when `IsolationMode=true` (`appsettings.Isolation.json`) |
| Real HMRC / Pension Regulator / Token Service | Pointed at WireMock catch-all; deep levy/AORN journeys need extra mappings under `tools/isolation/wiremock/mappings/` |

### Key files

- `docker-compose.yml` – single entry point
- `src/SFA.DAS.EmployerAccounts.Web/appsettings.Isolation.json` – FAKE config
- `src/SFA.DAS.EmployerAccounts.Web/Extensions/ConfigurationExtensions.cs` – `IsolationMode` bypasses Azure Table Storage
- `tools/isolation/wiremock/` – stub HTTP APIs
- `tools/isolation/sql/` – schema bootstrap + seed
