using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Commands.AcknowledgeTrainingProviderTask;
using SFA.DAS.EmployerAccounts.Exceptions;
using SFA.DAS.EmployerAccounts.Models.PAYE;
using SFA.DAS.EmployerAccounts.Queries.GetAccountById;
using SFA.DAS.EmployerAccounts.Queries.GetAccountPayeSchemes;
using SFA.DAS.EmployerAccounts.Queries.GetAccountsSinceDate;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerAccounts.Queries.GetPagedEmployerAccounts;
using SFA.DAS.EmployerAccounts.Queries.GetPayeSchemeByRef;
using SFA.DAS.EmployerAccounts.Queries.GetTeamMembers;
using SFA.DAS.EmployerAccounts.Queries.GetTeamMembersWhichReceiveNotifications;
using PayeScheme = SFA.DAS.EmployerAccounts.Api.Types.PayeScheme;

namespace SFA.DAS.EmployerAccounts.Api.Orchestrators;

public class AccountsOrchestrator(
    IMediator mediator,
    ILogger<AccountsOrchestrator> logger,
    IMapper mapper)
{
    public async Task<PayeScheme> GetPayeScheme(long accountId, string payeSchemeRef)
    {
        logger.LogInformation("Getting paye scheme {PayeSchemeRef} for account {AccountId}", payeSchemeRef, accountId);

        var payeSchemeResult = await mediator.Send(new GetPayeSchemeByRefQuery { AccountId = accountId, Ref = payeSchemeRef });
        return payeSchemeResult.PayeScheme == null ? null : ConvertToPayeScheme(accountId, payeSchemeResult);
    }

    public async Task<AccountDetail> GetAccount(long accountId)
    {
        logger.LogInformation("Getting account {AccountId}", accountId);

        var accountResult = await mediator.Send(new GetEmployerAccountDetailByIdQuery { AccountId = accountId });
        return accountResult.Account == null ? null : ConvertToAccountDetail(accountResult);
    }

    public async Task<AccountDetail> GetAccountById(long accountId)
    {
        logger.LogInformation("Getting account {AccountId}", accountId);

        var accountResult = await mediator.Send(new GetAccountByIdQuery { AccountId = accountId });
        return accountResult.Account == null ? null : ConvertToAccountDetail(accountResult);
    }

    public async Task<PagedApiResponse<Account>> GetAccounts(string toDate, int pageSize, int pageNumber)
    {
        logger.LogInformation("Getting all accounts.");

        toDate = toDate ?? DateTime.MaxValue.ToString("yyyyMMddHHmmss");

        var accountsResult = await mediator.Send(new GetPagedEmployerAccountsQuery { ToDate = toDate, PageSize = pageSize, PageNumber = pageNumber });

        var data = new List<Account>();

        accountsResult.Accounts.ForEach(account =>
        {
            var accountModel = new Account
            {
                AccountId = account.Id,
                AccountName = account.Name,
                HashedAccountId = account.HashedId,
                PublicAccountHashId = account.PublicHashedId,
                ApprenticeshipEmployerType = (ApprenticeshipEmployerType)account.ApprenticeshipEmployerType,
                AccountAgreementType = GetAgreementType(account.AccountLegalEntities.SelectMany(x => x.Agreements.Where(y => y.SignedDate.HasValue)).Select(x => x.Template.AgreementType).Distinct().ToList())
            };

            data.Add(accountModel);
        });

        return new PagedApiResponse<Account>
        {
            Data = data,
            Page = pageNumber,
            TotalPages = (accountsResult.AccountsCount / pageSize) + 1
        };
    }

    public async Task<List<TeamMember>> GetAccountTeamMembers(long accountId)
    {
        logger.LogInformation("Requesting team members for account {AccountId}", accountId);

        var teamMembers = await mediator.Send(new GetTeamMembersRequest(accountId));
        return teamMembers.TeamMembers.Select(x => mapper.Map<TeamMember>(x)).ToList();
    }

    public async Task<List<TeamMember>> GetAccountTeamMembersWhichReceiveNotifications(long accountId)
    {
        logger.LogInformation("Requesting team members which receive notifications for account {AccountId}", accountId);

        var teamMembers = await mediator.Send(new GetTeamMembersWhichReceiveNotificationsQuery { AccountId = accountId });
        return teamMembers.TeamMembersWhichReceiveNotifications.Select(x => mapper.Map<TeamMember>(x)).ToList();
    }

    public async Task<IEnumerable<PayeView>> GetPayeSchemesForAccount(long accountId)
    {
        try
        {
            var response = await mediator.Send(new GetAccountPayeSchemesQuery { AccountId = accountId });

            return response.PayeSchemes;
        }
        catch (InvalidRequestException)
        {
            return null;
        }
    }

    public async Task AcknowledgeTrainingProviderTask(long accountId)
    {
        var command = new AcknowledgeTrainingProviderTaskCommand(accountId);
        await mediator.Send(command);
    }

    private static PayeScheme ConvertToPayeScheme(long accountId, GetPayeSchemeByRefResponse payeSchemeResult)
    {
        return new PayeScheme
        {
            AccountId = accountId,
            Name = payeSchemeResult.PayeScheme.Name,
            Ref = payeSchemeResult.PayeScheme.Ref,
            AddedDate = payeSchemeResult.PayeScheme.AddedDate,
            RemovedDate = payeSchemeResult.PayeScheme.RemovedDate
        };
    }

    private static AccountDetail ConvertToAccountDetail(GetEmployerAccountDetailByIdResponse accountResult)
    {
        return new AccountDetail
        {
            AccountId = accountResult.Account.AccountId,
            HashedAccountId = accountResult.Account.HashedId,
            PublicHashedAccountId = accountResult.Account.PublicHashedId,
            DateRegistered = accountResult.Account.CreatedDate,
            OwnerEmail = accountResult.Account.OwnerEmail,
            DasAccountName = accountResult.Account.Name,
            LegalEntities = new ResourceList(accountResult.Account.LegalEntities.Select(x => new Resource { Id = x.ToString() })),
            PayeSchemes = new ResourceList(accountResult.Account.PayeSchemes.Select(x => new Resource { Id = x })),
            ApprenticeshipEmployerType = accountResult.Account.ApprenticeshipEmployerType.ToString(),
            AccountAgreementType = GetAgreementType(accountResult.Account.AccountAgreementTypes),
            AddTrainingProviderAcknowledged = accountResult.Account.AddTrainingProviderAcknowledged,
            NameConfirmed = accountResult.Account.NameConfirmed,
        };
    }

    private static AccountDetail ConvertToAccountDetail(GetAccountByIdResponse accountResult)
    {
        return new AccountDetail
        {
            AccountId = accountResult.Account.Id,
            HashedAccountId = accountResult.Account.HashedId,
            PublicHashedAccountId = accountResult.Account.PublicHashedId,
            DateRegistered = accountResult.Account.CreatedDate,
            DasAccountName = accountResult.Account.Name,
            ApprenticeshipEmployerType = accountResult.Account.ApprenticeshipEmployerType.ToString()
        };
    }

    private static AccountAgreementType GetAgreementType(List<AgreementType> agreementTypes)
    {
        if (agreementTypes is null || agreementTypes.Count == 0)
            return AccountAgreementType.Unknown;

        bool hasLevy = agreementTypes.Contains(AgreementType.Levy);
        bool hasNonLevyExpressionOfInterest = agreementTypes.Contains(AgreementType.NonLevyExpressionOfInterest);
        bool hasCombined = agreementTypes.Contains(AgreementType.Combined);

        return (hasLevy, hasNonLevyExpressionOfInterest, hasCombined) switch
        {
            (true, true, _) => AccountAgreementType.Combined,
            (_, _, true) => AccountAgreementType.Combined,
            (true, _, _) => AccountAgreementType.Levy,
            _ => AccountAgreementType.Unknown
        };
    }

    public async Task<PagedApiResponse<AccountNameSummary>> GetAccountsUpdated(DateTime sinceDate, int pageNumber, int pageSize)
    {
        logger.LogInformation("Getting accounts updated since {SinceDate}.", sinceDate);

        var response = await mediator.Send(new GetAccountsSinceDateQuery
        {
            SinceDate = sinceDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        });

        var totalPages = Math.Max((int)Math.Ceiling((double)response.Accounts.AccountsCount / pageSize), 1);

        return new PagedApiResponse<AccountNameSummary>
        {
            Data = response.Accounts.AccountList
                .Select(p => new AccountNameSummary
                {
                    AccountId = p.Id,
                    AccountName = p.Name
                })
                .ToList(),
            Page = pageNumber,
            TotalPages = totalPages
        };
    }
}
