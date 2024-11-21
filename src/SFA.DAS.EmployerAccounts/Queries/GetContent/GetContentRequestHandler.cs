using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Models.UserAccounts;

namespace SFA.DAS.EmployerAccounts.Queries.GetContent;

public class GetContentRequestHandler(
    IValidator<GetContentRequest> validator,
    ILogger<GetContentRequestHandler> logger,
    IContentApiClient contentApiClient,
    EmployerAccountsConfiguration employerAccountsConfiguration,
    IHttpContextAccessor httpContextAccessor,
    IAssociatedAccountsService associatedAccountsService)
    : IRequestHandler<GetContentRequest, GetContentResponse>
{
    public async Task<GetContentResponse> Handle(GetContentRequest message, CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        var hashedAccountId = httpContextAccessor.HttpContext.Request.RouteValues["HashedAccountId"]?.ToString().ToUpper();

        if (string.IsNullOrEmpty(hashedAccountId))
        {
            logger.LogInformation("GetContentRequestHandler HashedAccountId not found on route.");
            return new GetContentResponse();
        }

        logger.LogInformation("GetContentRequestHandler HashedAccountId: {Id}.", hashedAccountId);

        var levyStatus = await GetAccountLevyStatus(hashedAccountId);

        var applicationId = $"{employerAccountsConfiguration.ApplicationId}-{levyStatus.ToString().ToLower()}";

        logger.LogInformation("GetContentRequestHandler Fetching ContentBanner for applicationId: '{ApplicationId}'.", applicationId);

        try
        {
            var contentBanner = await contentApiClient.Get(message.ContentType, applicationId);

            logger.LogInformation("GetContentRequestHandler ContentBanner data: '{ContentBanner}'.", contentBanner);

            return new GetContentResponse
            {
                Content = contentBanner
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetContentRequestHandler Failed to get Content {ContentType} for {ApplicationId}", message.ContentType, applicationId);

            return new GetContentResponse
            {
                HasFailed = true
            };
        }
    }

    private async Task<ApprenticeshipEmployerType> GetAccountLevyStatus(string hashedAccountId)
    {
        var associatedAccounts = await associatedAccountsService.GetAccounts(forceRefresh: false);

        var hasEmployerAccountsClaims = associatedAccounts.TryGetValue(hashedAccountId, out var employerAccount);

        return hasEmployerAccountsClaims ? employerAccount.ApprenticeshipEmployerType : ApprenticeshipEmployerType.Unknown;
    }
}