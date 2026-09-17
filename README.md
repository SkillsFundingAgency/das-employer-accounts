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

Run the Employer Accounts **Web UI** with Docker dependencies and stubs so UI work does not need dependent services.

### What you get

| Piece | Role |
|-------|------|
| `sqlserver` + seed | SQL Server 2022; **publish the Database DACPAC first**, then `sql-init` applies isolation seed (`GP67XW`) |
| `redis` | Cache / data protection |
| `wiremock` | Outer API / Account API / org search / dashboard stubs |
| Host web | `./tools/isolation/scripts/run-web-host.sh` → `http://localhost:5024` |

Stub sign-in: `/service/SignIn-Stub` — Id `11111111-1111-1111-1111-111111111111`, email `isolation.employer@example.com`.

### Steps

```bash
# 1) Dependencies
docker compose up -d sqlserver redis wiremock

# 2) Schema — publish the existing SSDT project (same DACPAC as deploy).
#    Target: Server=localhost,1433; Database=EmployerAccounts;
#            User Id=sa; Password=Isolation_P@ssw0rd!; TrustServerCertificate=True
#    Tools: Visual Studio / Azure Data Studio publish, or sqlpackage against
#    src/SFA.DAS.EmployerAccounts.Database (build DACPAC in CI or locally on Windows/SSDT).
#    Do not use a partial tools/isolation SQL schema pack.

# 3) Seed only (requires EmployerAccounts DB + schema already present)
docker compose up sql-init

# 4) Web on the host (preferred)
./tools/isolation/scripts/run-web-host.sh
# UI: http://localhost:5024  (use 5024, not 5000 — AirPlay on macOS)
```

Set `IsolationMode=true` (the host script does). Config is `appsettings.Isolation.json` (WireMock URLs, Encoding, Docker SQL). Optional Azurite/LOCAL table storage still supplies **HMRC ClientId/Secret** — those keys are omitted from Isolation JSON on purpose.

### HMRC Gov Gateway (isolation)

Add-PAYE uses the **TEST** `das-hmrc-mock-api` (not WireMock). Defaults live in Isolation JSON (`BaseUrl` / `Scope`); **ClientId/Secret** come from local Azure Table Storage (Azurite) / das-employer-config for partition `LOCAL`. Run `az login` so Token Service empty-client-secret auth works. After table storage loads, Isolation JSON is applied again so Docker SQL / Encoding / WireMock URLs are not overwritten by a LOCAL row that points at cloud TEST.

### Notes

- No private NuGet is required for this isolation path.
- CDN: keep `cdn.url` / `CdnBaseUrl` as the AT front-end (`https://das-at-frnt-end.azureedge.net`) so GOV.UK CSS loads.
- For ngrok, tunnel `http://127.0.0.1:5024` and allow the public origin on the TEST HMRC OAuth client if callbacks fail.


## HMRC Gov Gateway (isolation)

Add-PAYE via Government Gateway must **not** hit WireMock `http://localhost:8080/hmrc/oauth/authorize` (that returns catch-all `{}`). WireMock is **not** used for HMRC OAuth anymore.

### Approach

Isolation points default `SFA.DAS.EmployerAccounts:Hmrc` BaseUrl/Scope at the **TEST** `das-hmrc-mock-api` (public hostname below). **Runtime** `ClientId`, `ClientSecret`, `BaseUrl`, and `Scope` are loaded from **local Azure Table Storage** (Azurite / Storage Emulator) when `ConfigurationStorageConnectionString` is set — the same path as normal local development, populated from **das-employer-config**. Do **not** paste those secrets into `appsettings.Isolation.json`.

If you already have the employer-accounts Configuration row for your `EnvironmentName` partition (Isolation uses **`LOCAL`**) in local table storage, that row supplies HMRC ClientId/Secret/BaseUrl/Scope — keep using it.

### TEST HMRC mock (Isolation defaults)

| Setting | Value |
| --- | --- |
| HMRC mock API | `https://test-hmrc-mock-api.apprenticeships.education.gov.uk/api/` |
| `Hmrc:Scope` (default) | `read:apprenticeship-levy` |
| Authorize (built as `{BaseUrl}oauth/authorize`) | `https://test-hmrc-mock-api.apprenticeships.education.gov.uk/api/oauth/authorize` |
| Token (built as `{BaseUrl}oauth/token`) | `https://test-hmrc-mock-api.apprenticeships.education.gov.uk/api/oauth/token` |

`HmrcService` builds authorize/token from **`BaseUrl` only**. If your local Configuration table row already has BaseUrl/Scope, those **override** the Isolation JSON defaults.

### Local table storage (ClientId / Secret / BaseUrl / Scope)

Same setup as earlier in this README (Azure Storage Emulator / Azurite + Configuration table from **das-employer-config**):

1. Keep Azurite / Storage Emulator running (do not wipe an existing working local store).
2. Ensure the employer-accounts config row is present for partition **`LOCAL`** (or your `EnvironmentName`), typically via das-employer-config updater.
3. `ConfigurationStorageConnectionString` defaults to `UseDevelopmentStorage=true`. Isolation mode **still loads** this table so HMRC values overlay FAKE Isolation JSON. After table storage loads, Isolation re-asserts local `DatabaseConnectionString` / Redis / WireMock API bases **and** Isolation `SFA.DAS.Encoding` salts (seeded account hash `GP67XW`) so a LOCAL config row that points at cloud TEST SQL / TEST Encoding does not break Docker SQL or hashed-id decode.

If add-PAYE shows `Unknown client id`, confirm the LOCAL row contains TEST HMRC `ClientId`/`Secret` and Azurite is running — not that you need to edit Isolation JSON.

### Redirect URI

Redirect URI is built at runtime for confirm PAYE, e.g. `http://localhost:5024/accounts/{hashedAccountId}/schemes/confirm`. That URI (and any **ngrok HTTPS origin** if you tunnel) may need to be allowed on the TEST mock OAuth client.

### Azure CLI (`az login`) for Token Service / Azure AD

Isolation `TokenServiceApi` uses **empty** `ClientId`/`ClientSecret` in JSON so token acquisition can use Azure CLI credentials locally (MI in Azure). `TokenServiceApi:IdentifierUri` still comes from table storage / das-employer-config when present.

```bash
az login
# Identity that can obtain tokens for TEST Token Service (IdentifierUri from config)
# and can reach the TEST HMRC mock over the network (VPN / allow-list as required)
```

Then rebuild/restart the isolation web host on port **5024**.

### Verify

1. Sign in via stub: `http://localhost:5024/service/SignIn-Stub`
2. Open add PAYE / Gov Gateway for the seeded isolation account
3. Browser must open `https://test-hmrc-mock-api.apprenticeships.education.gov.uk/...` (GG sign-in / grant scope), **not** `localhost:8080` JSON `{}`
4. Account home/teams should still work via WireMock stubs
