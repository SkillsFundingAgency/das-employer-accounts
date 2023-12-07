IF OBJECT_ID('employer_account.Account', 'U') IS NOT NULL
    AND OBJECT_ID('employer_account.EmployerAgreement', 'U') IS NOT NULL
BEGIN
UPDATE employer_account.Account
SET AddTrainingProviderAcknowledged = NULL;

UPDATE employer_account.EmployerAgreement
SET Acknowledged = NULL;
END