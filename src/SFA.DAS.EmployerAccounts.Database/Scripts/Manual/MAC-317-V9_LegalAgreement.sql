-- Set all v3 - v8 agreements to superseded (both pending and signed)
UPDATE [employer_account].[EmployerAgreement] 
SET    StatusId = 4,
	   ExpiredDate = GETDATE()
WHERE  StatusId IN (1, 2) AND TemplateId IN (3,4,5,6,7,8,9) 

UPDATE [employer_account].[AccountLegalEntity]
SET    SignedAgreementVersion = NULL, SignedAgreementId = NULL
WHERE  SignedAgreementVersion IN (3,4,5,6,7,8) AND Deleted IS NULL

UPDATE [employer_account].[AccountLegalEntity]
SET    PendingAgreementVersion = NULL, PendingAgreementId = NULL
WHERE  PendingAgreementVersion IN (3,4,5,6,7,8) AND Deleted IS NULL

-- Replace all pending v3 - v8 agreements with pending v8 agreement
UPDATE [employer_account].[AccountLegalEntity]
SET    PendingAgreementVersion = 9
WHERE  PendingAgreementVersion IN (3,4,5,6,7,8)

-- Create a pending v9 agreement for all account legal entities (except those that have them)
INSERT INTO [employer_account].[EmployerAgreement] (TemplateId, StatusId, AccountLegalEntityId)
SELECT 10, 1, Id
FROM   [employer_account].[AccountLegalEntity] 
WHERE  PendingAgreementVersion IS NULL AND PendingAgreementId IS NULL AND Deleted IS NULL

UPDATE ale
SET    PendingAgreementVersion = 9, PendingAgreementId = ea.Id
FROM   [employer_account].[AccountLegalEntity] ale 
JOIN   [employer_account].[EmployerAgreement] ea ON ea.AccountLegalEntityId = ale.Id AND ea.TemplateId = 10 AND ea.StatusId = 1
WHERE  PendingAgreementVersion IS NULL AND PendingAgreementId IS NULL AND Deleted IS NULL
