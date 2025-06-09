-- Description: Removes FirstName and LastName of users where they contain @ characters
-- Author: System
-- Date: 2024-03-21

BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE [employer_account].[User]
    SET FirstName = NULL,
        LastName = NULL
    WHERE FirstName LIKE '%@%' 
       OR LastName LIKE '%@%';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();

    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH; 