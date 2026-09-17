using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.EmployerAccounts.Web.Infrastructure.Isolation;

/// <summary>
/// Account API client for IsolationMode: calls WireMock/base URL with no Azure AD token.
/// </summary>
public class IsolationAccountApiClient : IAccountApiClient
{
    private readonly IAccountApiConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public IsolationAccountApiClient(IAccountApiConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient(nameof(IsolationAccountApiClient));
    }

    private string GetBaseUrl()
    {
        return _configuration.ApiBaseUrl.EndsWith("/")
            ? _configuration.ApiBaseUrl
            : _configuration.ApiBaseUrl + "/";
    }

    private async Task<T> Get<T>(string relativeUrl)
    {
        var url = GetBaseUrl() + relativeUrl.TrimStart('/');
        var json = await _httpClient.GetStringAsync(url);
        return JsonConvert.DeserializeObject<T>(json);
    }

    public Task<AccountDetailViewModel> GetAccount(string hashedAccountId)
        => Get<AccountDetailViewModel>($"api/accounts/{hashedAccountId}");

    public Task<AccountDetailViewModel> GetAccount(long accountId)
        => Get<AccountDetailViewModel>($"api/accounts/internal/{accountId}");

    public Task<ICollection<TeamMemberViewModel>> GetAccountUsers(string accountId)
        => Get<ICollection<TeamMemberViewModel>>($"api/accounts/{accountId}/users");

    public Task<ICollection<TeamMemberViewModel>> GetAccountUsers(long accountId)
        => Get<ICollection<TeamMemberViewModel>>($"api/accounts/internal/{accountId}/users");

    public Task<SFA.DAS.EAS.Account.Api.Types.EmployerAgreementView> GetEmployerAgreement(string accountId, string legalEntityId, string agreementId)
        => Get<SFA.DAS.EAS.Account.Api.Types.EmployerAgreementView>($"api/accounts/{accountId}/legalEntities/{legalEntityId}/agreements/{agreementId}/agreement");

    public Task<ICollection<ResourceViewModel>> GetLegalEntitiesConnectedToAccount(string accountId)
        => Get<ICollection<ResourceViewModel>>($"api/accounts/{accountId}/legalentities");

    public Task<LegalEntityViewModel> GetLegalEntity(string accountId, long id)
        => Get<LegalEntityViewModel>($"api/accounts/{accountId}/legalentities/{id}");

    public Task<ICollection<LevyDeclarationViewModel>> GetLevyDeclarations(string accountId)
        => Get<ICollection<LevyDeclarationViewModel>>($"api/accounts/{accountId}/levy");

    public Task<PagedApiResponseViewModel<SFA.DAS.EAS.Account.Api.Types.AccountLegalEntityViewModel>> GetPageOfAccountLegalEntities(int pageNumber = 1, int pageSize = 1000)
        => Get<PagedApiResponseViewModel<SFA.DAS.EAS.Account.Api.Types.AccountLegalEntityViewModel>>($"api/accountlegalentities?pageNumber={pageNumber}&pageSize={pageSize}");

    public Task<PagedApiResponseViewModel<AccountWithBalanceViewModel>> GetPageOfAccounts(int pageNumber = 1, int pageSize = 1000, DateTime? toDate = null)
        => Get<PagedApiResponseViewModel<AccountWithBalanceViewModel>>($"api/accounts?pageNumber={pageNumber}&pageSize={pageSize}");

    public Task<ICollection<ResourceViewModel>> GetPayeSchemesConnectedToAccount(string accountId)
        => Get<ICollection<ResourceViewModel>>($"api/accounts/{accountId}/payeschemes");

    public Task<T> GetResource<T>(string uri)
        => Get<T>(uri);

    public Task<StatisticsViewModel> GetStatistics()
        => Get<StatisticsViewModel>("api/statistics");

    public Task<TransactionsViewModel> GetTransactions(string accountId, int year, int month)
        => Get<TransactionsViewModel>($"api/accounts/{accountId}/transactions/{year}/{month}");

    public Task<ICollection<TransactionSummaryViewModel>> GetTransactionSummary(string accountId)
        => Get<ICollection<TransactionSummaryViewModel>>($"api/accounts/{accountId}/transactions");

    public Task<ICollection<TransferConnectionViewModel>> GetTransferConnections(string accountHashedId)
        => Get<ICollection<TransferConnectionViewModel>>($"api/accounts/{accountHashedId}/transferconnections");

    public Task<ICollection<AccountDetailViewModel>> GetUserAccounts(string userId)
        => Get<ICollection<AccountDetailViewModel>>($"api/user/{userId}/accounts");

    public async Task Ping()
    {
        var url = GetBaseUrl() + "api/ping";
        using var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
    }

    public Task<ICollection<LegalEntityViewModel>> GetLegalEntityDetailsConnectedToAccount(string accountId)
        => Get<ICollection<LegalEntityViewModel>>($"api/accounts/{accountId}/legalentities?includeDetails=true");
}
