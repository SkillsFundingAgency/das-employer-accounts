/*
Isolation post-deploy: same as Database PostDeployment.sql but without SeedDevData.
SeedDevData uses TEST encoding hashes (JRML7V); isolation uses GP67XW via 02-seed.sql.
*/
:r ../../../src/SFA.DAS.EmployerAccounts.Database/Scripts/PostDeployment/MAC-95_Remove_transfer_connections.sql
:r ../../../src/SFA.DAS.EmployerAccounts.Database/Scripts/PostDeployment/CreateAgreementTemplates.sql
:r ../../../src/SFA.DAS.EmployerAccounts.Database/Scripts/PostDeployment/AML-3762-EOI-API.sql
:r ../../../src/SFA.DAS.EmployerAccounts.Database/Scripts/PostDeployment/UpdateAgreementTemplateV3.sql
:r ../../../src/SFA.DAS.EmployerAccounts.Database/Scripts/PostDeployment/APPMAN-1457-RemoveInvalidUserNames.sql
:r ../../../src/SFA.DAS.EmployerAccounts.Database/Scripts/PostDeployment/AML-2119-RestoreAgreementDetails.sql
