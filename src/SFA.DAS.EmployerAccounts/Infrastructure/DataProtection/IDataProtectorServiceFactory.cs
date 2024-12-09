namespace SFA.DAS.EmployerAccounts.Infrastructure.DataProtection;

public interface IDataProtectorServiceFactory
{
    IDataProtectorService Create(string key);
}

