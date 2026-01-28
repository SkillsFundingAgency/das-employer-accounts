-- Set all v3 - v9 agreements to superseded (both pending and signed)
UPDATE [employer_account].[EmployerAgreement] 
SET    StatusId = 4,
	   ExpiredDate = GETDATE()
WHERE  StatusId IN (1, 2) AND TemplateId IN (4,5,6,7,8,9,10) 

UPDATE [employer_account].[AccountLegalEntity]
SET    SignedAgreementVersion = NULL, SignedAgreementId = NULL
WHERE  SignedAgreementVersion IN (3,4,5,6,7,8,9) AND Deleted IS NULL

UPDATE [employer_account].[AccountLegalEntity]
SET    PendingAgreementVersion = NULL, PendingAgreementId = NULL
WHERE  PendingAgreementVersion IN (3,4,5,6,7,8,9) AND Deleted IS NULL

-- Replace all pending v3 - v9 agreements with pending v10 agreement
UPDATE [employer_account].[AccountLegalEntity]
SET    PendingAgreementVersion = 10
WHERE  PendingAgreementVersion IN (3,4,5,6,7,8,9)

-- Create a pending v10 agreement for all account legal entities (except those that have them)
-- Set Acknowledged = 1 since these are only created where there was a signed previous agreement
INSERT INTO [employer_account].[EmployerAgreement] (TemplateId, StatusId, AccountLegalEntityId, Acknowledged)
SELECT 11, 1, Id, 1
FROM   [employer_account].[AccountLegalEntity] 
WHERE  PendingAgreementVersion IS NULL AND PendingAgreementId IS NULL AND Deleted IS NULL

UPDATE ale
SET    PendingAgreementVersion = 10, PendingAgreementId = ea.Id
FROM   [employer_account].[AccountLegalEntity] ale 
JOIN   [employer_account].[EmployerAgreement] ea ON ea.AccountLegalEntityId = ale.Id AND ea.TemplateId = 11 AND ea.StatusId = 1
WHERE  PendingAgreementVersion IS NULL AND PendingAgreementId IS NULL AND Deleted IS NULL
