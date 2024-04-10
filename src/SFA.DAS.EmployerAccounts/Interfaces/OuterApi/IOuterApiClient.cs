namespace SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

public interface IOuterApiClient
{
    Task<TResponse> Get<TResponse>(IGetApiRequest request);
    Task Post(IPostApiRequest request);
}