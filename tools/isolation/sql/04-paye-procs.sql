-- Isolation: PAYE / AccountHistory stored procedures needed for add-PAYE (Gov Gateway)
-- Sourced from src/SFA.DAS.EmployerAccounts.Database/StoredProcedures

-- ===== GetPayeSchemesInUse.sql =====
CREATE OR ALTER PROCEDURE [employer_account].[GetPayeSchemesInUse]
	@payeRef varchar(20)
AS
	SELECT 
		* 
	FROM 
		[employer_account].[Paye] p
	inner join 
		[employer_account].[AccountHistory] ah on ah.PayeRef = p.Ref
	WHERE
		ah.RemovedDate is null and p.Ref = @payeRef
GO

-- ===== GetPayeSchemes_ByAccountId.sql =====
CREATE OR ALTER PROCEDURE [employer_account].[GetPayeSchemes_ByAccountId]
	@accountId BIGINT
AS
	SELECT
        Ref
        ,[Name]
        ,AccountId
	FROM 
		[employer_account].[Paye] p
	inner join 
		[employer_account].[AccountHistory] ah on ah.PayeRef = p.Ref
	WHERE
		ah.AccountId = @accountId and ah.RemovedDate is null
GO

-- ===== GetPayeSchemesAddedByGovernmentGateway_ByAccountId.sql =====
CREATE OR ALTER PROCEDURE [employer_account].[GetPayeSchemesAddedByGovernmentGateway_ByAccountId]
	@accountId BIGINT
AS
	SELECT 
		* 
	FROM 
		[employer_account].[Paye] p
	INNER JOIN 
		[employer_account].[AccountHistory] ah on ah.PayeRef = p.Ref
	WHERE
		ah.AccountId = @accountId 
		AND ah.RemovedDate IS NULL
		AND p.Aorn IS NULL
GO

-- ===== GetPaye_ByRef.sql =====
CREATE OR ALTER PROCEDURE [employer_account].[GetPaye_ByRef]
	@Ref NVARCHAR(16)
AS

Select TOP 1
	paye.Ref as Ref,
    paye.Name as RefName
from 
	employer_account.Paye paye
WHERE
	paye.Ref = @Ref
GO

-- ===== GetPayeForAccount_ByRef.sql =====
CREATE OR ALTER PROCEDURE employer_account.GetPayeForAccount_ByRef
	@HashedAccountId NVARCHAR(100),
	@Ref NVARCHAR(16)
AS
Select TOP 1
	paye.Ref,
    paye.Name,
	ah.AddedDate,
	ah.RemovedDate
from employer_account.Paye paye
INNER JOIN employer_account.AccountHistory ah ON ah.PayeRef = paye.Ref
INNER JOIN employer_account.Account a ON a.Id = ah.AccountId
WHERE a.HashedId = @HashedAccountId AND paye.Ref = @Ref
ORDER BY ah.Id DESC
GO

-- ===== CreatePaye.sql =====
CREATE OR ALTER PROCEDURE [employer_account].[CreatePaye]
	@employerRef NVARCHAR(16),
	@accessToken VARCHAR(50),
	@refreshToken VARCHAR(50),
	@employerRefName VARCHAR(500) NULL,
	@aorn VARCHAR(50) NULL
AS
BEGIN
	INSERT INTO [employer_account].[Paye](Ref, AccessToken, RefreshToken, Name, Aorn) 
	VALUES (@employerRef, @accessToken, @refreshToken, @employerRefName, @aorn);
END
GO

-- ===== UpdatePaye.sql =====
CREATE OR ALTER PROCEDURE [employer_account].[UpdatePaye]
	@employerRef NVARCHAR(16),
	@accessToken VARCHAR(50),
	@refreshToken VARCHAR(50),
	@employerRefName VARCHAR(500) NULL,
	@aorn VARCHAR(50) NULL
AS
BEGIN
	UPDATE 
		[employer_account].[Paye] 
	SET 
		AccessToken = @accessToken, 
		RefreshToken = @refreshToken,
		Name = @employerRefName,
		Aorn = @aorn
	WHERE 
		Ref = @employerRef

END
GO

-- ===== UpdatePayeName_ByRef.sql =====
CREATE OR ALTER PROCEDURE [employer_account].[UpdatePayeName_ByRef]
	@Ref varchar(16) = 0,
	@RefName varchar(500)
AS
	
	Update employer_account.Paye 
	set Name = @RefName 
	where Ref=@Ref
GO

-- ===== CreateAccountHistory.sql =====
CREATE OR ALTER PROCEDURE [employer_account].[CreateAccountHistory]
	@AccountId BIGINT,
	@PayeRef VARCHAR(20),
	@AddedDate DATETIME
AS
	INSERT INTO [employer_account].[AccountHistory] (AccountId, PayeRef, AddedDate)
	VALUES (@AccountId, @PayeRef, @AddedDate)
GO

-- ===== UpdateAccountHistory.sql =====
CREATE OR ALTER PROCEDURE [employer_account].[UpdateAccountHistory]
	@AccountId BIGINT,
	@PayeRef VARCHAR(20),
	@RemovedDate DATETIME
AS
	UPDATE [employer_account].[AccountHistory] SET RemovedDate = @RemovedDate 
	WHERE AccountId = @AccountId AND PayeRef = @PayeRef AND RemovedDate IS NULL
GO

-- ===== AddPayeToAccount.sql =====
CREATE OR ALTER PROCEDURE [employer_account].[AddPayeToAccount]
	@accountId BIGINT,
	@employerRef  NVARCHAR(16),
	@accessToken VARCHAR(30),
	@refreshToken VARCHAR(30),
	@addedDate DATETIME,
	@employerRefName VARCHAR(500) NULL,
	@aorn VARCHAR(50) NULL
AS
BEGIN
	DECLARE @employerAgreementId BIGINT

	
	IF EXISTS(select 1 from [employer_account].[Paye] where Ref = @employerRef)
	BEGIN
		EXEC [employer_account].[UpdatePaye] @employerRef,@accessToken, @refreshToken, @employerRefName, @aorn
	END
	ELSE
	BEGIN
		EXEC [employer_account].[CreatePaye] @employerRef,@accessToken, @refreshToken, @employerRefName, @aorn
	END

	EXEC [employer_account].[CreateAccountHistory] @accountId,@employerRef,@addedDate

END
GO

