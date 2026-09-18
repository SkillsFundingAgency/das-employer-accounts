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

Run the Employer Accounts **Web UI** alone (Docker SQL + Redis + WireMock). No other microservices.

### Prerequisites

- Docker Desktop
- A **.NET 10 runtime** to host the site (`dotnet --list-runtimes` should show `Microsoft.AspNetCore.App 10.x`).  
  If `~/.dotnet/dotnet` SIGKILLs on macOS, use `/usr/local/share/dotnet` or a copy under `~/dotnet-isolation`.
- A built **Database DACPAC** (classic SSDT project — build in Visual Studio / CI; it does not build on macOS).  
  Optional: `dotnet tool install -g microsoft.sqlpackage`

### 1. Start dependencies

```bash
docker compose up -d sqlserver redis wiremock
```

### 2. Publish schema, then seed

```bash
# Once per machine / after schema changes — point at your built .dacpac
./tools/isolation/scripts/publish-dacpac.sh /path/to/SFA.DAS.EmployerAccounts.Database.dacpac

# Seed stub user + account GP67XW (safe to re-run)
docker compose up sql-init
```

Connection used by publish/seed: `localhost,1433` / database `EmployerAccounts` / `sa` / `Isolation_P@ssw0rd!`.

### 3. Build and run the web UI

```bash
# Builds with the official SDK container (fixes Mac host SDK issues), then runs the DLL
./tools/isolation/scripts/rebuild-web-docker.sh
./tools/isolation/scripts/run-web-host.sh
```

Open **http://localhost:5024** (not 5000 — AirPlay on macOS).

Stub sign-in: **http://localhost:5024/service/SignIn-Stub**  
- Id: `11111111-1111-1111-1111-111111111111`  
- Email: `isolation.employer@example.com`

Then open **http://localhost:5024/accounts/GP67XW/teams**.

`IsolationMode=true` is set by the run script. Config: `src/SFA.DAS.EmployerAccounts.Web/appsettings.Isolation.json`.

### What works in this mode

| Journey | How |
|---------|-----|
| Home / team / orgs & agreements | Seed + WireMock Account API |
| Sign employer agreement | In-app |
| Add organisation | WireMock org search |
| Add PAYE via Pensions Regulator (AORN) | WireMock `/tpr/...` |
| Add PAYE via HMRC Gov Gateway | TEST HMRC mock (below) |

### HMRC Gov Gateway (optional)

Add-PAYE via Gateway uses the **TEST** `das-hmrc-mock-api` (not WireMock).

1. `az login` (Token Service empty-client-secret auth).
2. Put HMRC **ClientId/Secret** in LOCAL Azure Table Storage (Azurite / das-employer-config). They are **not** in Isolation JSON on purpose.
3. Isolation JSON is loaded again after table storage so Docker SQL / Encoding / WireMock URLs stay local.
4. Mock login used in verification: user `LE_12_1000` / password `password`.
5. Optional ngrok: tunnel `http://127.0.0.1:5024`; allow the public origin on the TEST HMRC OAuth client if the callback fails.

### Notes

- No private NuGet feed is required for this path.
- CDN stays on AT (`das-at-frnt-end`) so GOV.UK CSS loads.
- Shared UI menu links target `*.local-eas`; isolation rewrites them to the current host (localhost / ngrok).

