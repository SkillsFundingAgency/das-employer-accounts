using System.Threading;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Accounts;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.Accounts;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Queries.GetCreateAccountTaskList;

public class GetCreateAccountTaskListQueryHandler(IOuterApiClient apiClient) : IRequestHandler<GetCreateAccountTaskListQuery, GetCreateAccountTaskListQueryResponse>
{
    public async Task<GetCreateAccountTaskListQueryResponse> Handle(GetCreateAccountTaskListQuery request, CancellationToken cancellationToken)
    {
        var response = await apiClient.Get<GetCreateTaskListResponse>(
            new GetCreateTaskListRequest(
                request.AccountId,
                request.HashedAccountId,
                request.UserRef)
        );

        return new GetCreateAccountTaskListQueryResponse
        {
            HashedAccountId = response.HashedAccountId,
            HasSignedAgreement = response.HasSignedAgreement,
            AgreementAcknowledged = response.AgreementAcknowledged,
            HasProviders = response.HasProviders,
            NameConfirmed = response.NameConfirmed,
            HasPayeScheme = response.HasPayeScheme,
            HasProviderPermissions = response.HasProviderPermissions,
            PendingAgreementId = response.PendingAgreementId,
            UserFirstName = response.UserFirstName,
            UserLastName = response.UserLastName,
            AddTrainingProviderAcknowledged = response.AddTrainingProviderAcknowledged,
        };
    }
}