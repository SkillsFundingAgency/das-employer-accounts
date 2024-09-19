using System.Threading;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Organisation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntityRemove;

public class GetAccountLegalEntityRemoveQueryHandler(
    IValidator<GetAccountLegalEntityRemoveRequest> validator,
    IEmployerAgreementRepository employerAgreementRepository,
    IEncodingService encodingService,
    ICommitmentsV2ApiClient commitmentV2ApiClient)
    : IRequestHandler<GetAccountLegalEntityRemoveRequest, GetAccountLegalEntityRemoveResponse>
{
    public async Task<GetAccountLegalEntityRemoveResponse> Handle(GetAccountLegalEntityRemoveRequest message, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        if (validationResult.IsUnauthorized)
        {
            throw new UnauthorizedAccessException();
        }

        var accountId = encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
        var accountLegalEntityId = encodingService.Decode(message.HashedAccountLegalEntityId, EncodingType.PublicAccountLegalEntityId);
        var accountLegalEntity = await employerAgreementRepository.GetAccountLegalEntity(accountLegalEntityId);

        var result = await employerAgreementRepository.GetAccountLegalEntityAgreements(accountLegalEntityId);
        if (result == null)
        {
            return new GetAccountLegalEntityRemoveResponse();
        }

        if (result.Any(x => x.SignedDate.HasValue))
        {
            return new GetAccountLegalEntityRemoveResponse
            {
                CanBeRemoved = await SetRemovedStatusBasedOnCommitments(accountId, accountLegalEntity),
                HasSignedAgreement = true,
                Name = accountLegalEntity.Name
            };
        }

        return new GetAccountLegalEntityRemoveResponse
        {
            CanBeRemoved = true,
            HasSignedAgreement = false,
            Name = accountLegalEntity.Name
        };
    }

    private async Task<bool> SetRemovedStatusBasedOnCommitments(long accountId, AccountLegalEntityModel accountLegalEntityModel)
    {
        var commitments = await commitmentV2ApiClient.GetEmployerAccountSummary(accountId);

        var commitmentConnectedToEntity = commitments.ApprenticeshipStatusSummaryResponse.FirstOrDefault(c =>
            !string.IsNullOrEmpty(c.LegalEntityIdentifier)
            && c.LegalEntityIdentifier.Equals(accountLegalEntityModel.Identifier)
            && c.LegalEntityOrganisationType == accountLegalEntityModel.OrganisationType);

        return commitmentConnectedToEntity == null || HasNoActiveApprenticeships(commitmentConnectedToEntity);
    }

    private static bool HasNoActiveApprenticeships(ApprenticeshipStatusSummaryResponse commitmentConnectedToEntity)
    {
        return commitmentConnectedToEntity.ActiveCount +
            commitmentConnectedToEntity.PendingApprovalCount +
            commitmentConnectedToEntity.WithdrawnCount +
            commitmentConnectedToEntity.CompletedCount +
            commitmentConnectedToEntity.PausedCount == 0;
    }
}