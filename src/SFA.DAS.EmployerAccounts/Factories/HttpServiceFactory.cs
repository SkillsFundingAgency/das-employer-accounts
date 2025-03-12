using SFA.DAS.Api.Common.Interfaces;

namespace SFA.DAS.EmployerAccounts.Factories;

public class HttpServiceFactory(IAzureClientCredentialHelper azureClientCredentialHelper) : IHttpServiceFactory
{
    public IHttpService Create(string identifierUri)
    {
        return new HttpService(identifierUri, azureClientCredentialHelper);
    }
}