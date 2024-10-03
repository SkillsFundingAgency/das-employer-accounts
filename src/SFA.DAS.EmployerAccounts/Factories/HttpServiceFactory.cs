namespace SFA.DAS.EmployerAccounts.Factories;

public class HttpServiceFactory : IHttpServiceFactory
{
    public IHttpService Create(string identifierUri)
    {
        return new HttpService(identifierUri);
    }
}