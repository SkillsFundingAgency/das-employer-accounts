namespace SFA.DAS.EmployerAccounts.Factories;

public class HttpServiceFactory : IHttpServiceFactory
{       
    public IHttpService Create(string identifierUri)
    {
        var client = new HttpService( identifierUri);
        return client;
    }
}