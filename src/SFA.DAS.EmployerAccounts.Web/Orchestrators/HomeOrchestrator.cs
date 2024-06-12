using SFA.DAS.EmployerAccounts.Commands.UnsubscribeProviderEmail;
using SFA.DAS.EmployerAccounts.Commands.UpsertRegisteredUser;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.EmployerAccounts.Queries.GetUserAccounts;
using SFA.DAS.EmployerAccounts.Queries.GetUserInvitations;
using SFA.DAS.EmployerAccounts.Web.Extensions;

namespace SFA.DAS.EmployerAccounts.Web.Orchestrators;

public class HomeOrchestrator : IHomeOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILogger<HomeOrchestrator> _logger;

    public HomeOrchestrator(IMediator mediator, ILogger<HomeOrchestrator> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public virtual async Task<OrchestratorResponse<UserAccountsViewModel>> GetUserAccounts(
        string userId,
        GaQueryData gaQueryData = null,
        string redirectUri = null,
        List<RedirectUriConfiguration> validRedirectUris = null,
        DateTime? LastTermsAndConditionsUpdate = null)
    {
        var getUserAccountsQueryResponse = await _mediator.Send(new GetUserAccountsQuery
        {
            UserRef = userId
        });

        var getUserInvitationsResponse = await _mediator.Send(new GetNumberOfUserInvitationsQuery
        {
            UserId = userId
        });

        var getUserQueryResponse = await _mediator.Send(new GetUserByRefQuery
        {
            UserRef = userId
        });

        var validatedRedirectUri = ValidateRedirectUri(redirectUri, validRedirectUris);

        return new OrchestratorResponse<UserAccountsViewModel>
        {
            Data = new UserAccountsViewModel
            {
                Accounts = getUserAccountsQueryResponse.Accounts,
                RedirectUri = validatedRedirectUri.RedirectUri,
                RedirectDescription = validatedRedirectUri.Description,
                GaQueryData = gaQueryData,
                Invitations = getUserInvitationsResponse.NumberOfInvites,
                TermAndConditionsAcceptedOn = getUserQueryResponse.User.TermAndConditionsAcceptedOn,
                LastTermsAndConditionsUpdate = LastTermsAndConditionsUpdate
            }
        };
    }

    public virtual async Task<OrchestratorResponse<ProviderInvitationViewModel>> GetProviderInvitation(Guid correlationId)
    {
        var getProviderInvitationQueryResponse = await _mediator.Send(new GetProviderInvitationQuery
        {
            CorrelationId = correlationId
        });

        if (getProviderInvitationQueryResponse.Result != null)
        {
            return new OrchestratorResponse<ProviderInvitationViewModel>
            {
                Data = new ProviderInvitationViewModel
                {
                    EmployerEmail = getProviderInvitationQueryResponse.Result.EmployerEmail,
                    EmployerFirstName = getProviderInvitationQueryResponse.Result.EmployerFirstName,
                    EmployerLastName = getProviderInvitationQueryResponse.Result.EmployerLastName,
                    EmployerOrganisation = getProviderInvitationQueryResponse.Result.EmployerOrganisation,
                    Ukprn = getProviderInvitationQueryResponse.Result.Ukprn,
                    Status = getProviderInvitationQueryResponse.Result.Status
                }
            };
        }

        return new OrchestratorResponse<ProviderInvitationViewModel>();
    }

    public virtual async Task Unsubscribe(Guid correlationId)
    {
        await _mediator.Send(new UnsubscribeProviderEmailCommand
        {
            CorrelationId = correlationId
        });
    }

    public virtual async Task SaveUpdatedIdentityAttributes(string userRef, string email, string firstName, string lastName, string correlationId = null)
    {
        await _mediator.Send(new UpsertRegisteredUserCommand
        {
            EmailAddress = email,
            UserRef = userRef,
            LastName = lastName,
            FirstName = firstName,
            CorrelationId = correlationId
        });
    }

    public virtual async Task UpdateTermAndConditionsAcceptedOn(string userRef)
    {
        await _mediator.Send(new UpdateTermAndConditionsAcceptedOnCommand
        {
            UserRef = userRef
        });
    }

    public virtual async Task<User> GetUser(string userRef)
    {
        try
        {
            var user = await _mediator.Send(new GetUserByRefQuery
            {
                UserRef = userRef
            });
            return user.User;
        }
        catch (InvalidRequestException)
        {
            return null;
        }
    }

    private (string RedirectUri, string Description) ValidateRedirectUri(string redirectUri, List<RedirectUriConfiguration> validRedirectUris)
    {
        if (validRedirectUris != null && Uri.TryCreate(redirectUri, UriKind.Absolute, out Uri uri))
        {
            var validRedirectUri = validRedirectUris.Find(p => p.Uri == uri.RemoveQuery());
            if (validRedirectUri != null)
            {
                _logger.LogInformation($"Redirect URI matched with a valid redirect URI.");
                return (redirectUri, validRedirectUri.Description);
            }
        }

        return (null, null);
    }
}