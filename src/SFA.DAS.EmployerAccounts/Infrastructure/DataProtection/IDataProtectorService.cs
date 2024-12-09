namespace SFA.DAS.EmployerAccounts.Infrastructure.DataProtection;

public interface IDataProtectorService
{
    string Protect(string plainText);
    string Unprotect(string cipherText);
}

