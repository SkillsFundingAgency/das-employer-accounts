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
    IHttpContextAccessor httpContextAccessor)
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
        
        var levyStatus = GetAccountLevyStatus(hashedAccountId);

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

    private ApprenticeshipEmployerType GetAccountLevyStatus(string hashedAccountId)
    {
        var employerAccountClaim = httpContextAccessor.HttpContext.User.FindFirst(EmployerClaims.AccountsClaimsTypeIdentifier);
        
        logger.LogInformation("GetContentRequestHandler AccountsClaimsTypeIdentifier Claims: '{Accounts}'.", employerAccountClaim);

        Dictionary<string, EmployerUserAccountItem> employerAccounts;

        try
        {
            employerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(employerAccountClaim.Value);
        }
        catch (JsonSerializationException exception)
        {
            logger.LogError(exception, "Could not deserialize employer account claim for user");
            throw;
        }
        
        logger.LogInformation("GetContentRequestHandler EmployerAccounts: '{Accounts}'.", JsonConvert.SerializeObject(employerAccounts));

        var employerAccount = employerAccounts.Single(x => x.Key == hashedAccountId).Value;

        return employerAccount.ApprenticeshipEmployerType;
    }
}