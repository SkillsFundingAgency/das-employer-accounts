using SFA.DAS.EmployerAccounts.Models.UserProfile;

namespace SFA.DAS.EmployerAccounts.Web.Orchestrators;

public interface IHomeOrchestrator
{
    Task<OrchestratorResponse<ProviderInvitationViewModel>> GetProviderInvitation(Guid correlationId);
    Task<User> GetUser(string userRef);
    Task<OrchestratorResponse<UserAccountsViewModel>> GetUserAccounts(string userId, GaQueryData gaQueryData = null, string redirectUri = null, List<RedirectUriConfiguration> validRedirectUris = null, DateTime? LastTermsAndConditionsUpdate = null);
    Task SaveUpdatedIdentityAttributes(string userRef, string email, string firstName, string lastName, string correlationId = null);
    Task Unsubscribe(Guid correlationId);
    Task UpdateTermAndConditionsAcceptedOn(string userRef);
}