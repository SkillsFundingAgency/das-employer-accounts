using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Models.UserAccounts;
using InvalidRequestException = SFA.DAS.EmployerAccounts.Exceptions.InvalidRequestException;

namespace SFA.DAS.EmployerAccounts.Queries.GetContent;

public class GetContentRequestHandler : IRequestHandler<GetContentRequest, GetContentResponse>
{
    private readonly IValidator<GetContentRequest> _validator;
    private readonly ILogger<GetContentRequestHandler> _logger;
    private readonly IContentApiClient _contentApiClient;
    private readonly EmployerAccountsConfiguration _employerAccountsConfiguration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetContentRequestHandler(
        IValidator<GetContentRequest> validator,
        ILogger<GetContentRequestHandler> logger,
        IContentApiClient contentApiClient,
        EmployerAccountsConfiguration employerAccountsConfiguration,
        IHttpContextAccessor httpContextAccessor)
    {
        _validator = validator;
        _logger = logger;
        _contentApiClient = contentApiClient;
        _employerAccountsConfiguration = employerAccountsConfiguration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GetContentResponse> Handle(GetContentRequest message, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        var accountIdFromUrl = _httpContextAccessor.HttpContext.Request.RouteValues["HashedAccountId"]?.ToString().ToUpper();

        if (string.IsNullOrEmpty(accountIdFromUrl))
        {
            return new GetContentResponse();
        }
        
        var levyStatus = GetAccountLevyStatus(accountIdFromUrl);

        var applicationIdWithLevyStatus = $"{_employerAccountsConfiguration.ApplicationId}-{levyStatus.ToString().ToLower()}";

        var applicationId = message.UseLegacyStyles ? $"{applicationIdWithLevyStatus}-legacy" : applicationIdWithLevyStatus;

        try
        {
            var contentBanner = await _contentApiClient.Get(message.ContentType, applicationId);

            return new GetContentResponse
            {
                Content = contentBanner
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Content {ContentType} for {ApplicationId}", message.ContentType, applicationId);

            return new GetContentResponse
            {
                HasFailed = true
            };
        }
    }

    private ApprenticeshipEmployerType GetAccountLevyStatus(string accountIdFromUrl)
    {
        var employerAccountClaim = _httpContextAccessor.HttpContext.User.FindFirst(EmployerClaims.AccountsClaimsTypeIdentifier);

        Dictionary<string, EmployerUserAccountItem> employerAccounts;

        try
        {
            employerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(employerAccountClaim.Value);
        }
        catch (JsonSerializationException exception)
        {
            _logger.LogError(exception, "Could not deserialize employer account claim for user");
            throw;
        }

        var employerAccount = employerAccounts.Single(x => x.Key == accountIdFromUrl).Value;

        return employerAccount.ApprenticeshipEmployerType;
    }
}